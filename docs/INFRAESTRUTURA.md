# Infraestrutura

Onde cada coisa roda, e onde cada coisa é definida.

> Este documento descrevia, até 04/09, um cluster **kind** local como se fosse a
> infraestrutura do projeto. Não é mais: a infraestrutura é a AWS, e o kind
> sobrou como ambiente de desenvolvimento. Reescrito na
> [#65](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/65).

## Os três ambientes, e o quarto que não é ambiente

| | Onde roda | Banco | Quem publica |
|---|---|---|---|
| `dev`, `hom`, `prod` | EKS, na AWS: OS, Billing e Execução no namespace `oficina-mecanica` | RDS do OS, RDS da Execução e a tabela DynamoDB `tc-grupo160-<amb>-billing` | workflow `Pipeline` de cada serviço (push em `homolog`/`main`, disparo manual para `dev`) ou `scripts/sobe-tudo.sh` |
| Desenvolvimento local | kind, na sua máquina | PostgreSQL dentro do cluster | você, à mão |
| Só rodar a API | Docker Compose | contêiner do Compose | você, à mão |

A última linha é a que resolve o dia a dia. **Se você só quer a API de pé, use o
Docker Compose** — está no [SETUP.md](SETUP.md) e sobe em segundos. O kind serve
quando o que você está mexendo é o Kubernetes em si.

## Onde a infraestrutura é definida

Este repositório é o OS Service. Além da aplicação, ele mantém a infraestrutura
de mensageria que pertence ao serviço. Rede e bancos continuam em repositórios
próprios:

| Repositório | O que define |
|---|---|
| [tech-challenge-infra-k8s](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s) | VPC, EKS, API Gateway (com a rota pública do webhook do Mercado Pago), ALB com regras por serviço, ECR, segredos (JWT e Mercado Pago), manifests do OS Service, Cluster Autoscaler — e o `local/`, com o kind |
| [tech-challenge-infra-database](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-database) | RDS do OS (Multi-AZ) e RDS da Execução (Single-AZ em `dev` e `hom`, Multi-AZ em `prod`), subnet group, uma credencial por banco no Secrets Manager |
| Este repositório, [`infra/`](../infra/README.md) | Tópico SNS do OS Service, filas SQS que consomem Billing/Execução, DLQs e configuração no Secrets Manager |
| [Billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing/tree/main/infra) | Tópico SNS do Billing, filas de eventos e comandos, DLQs e configuração, tabela DynamoDB (F4-13); manifests `k8s/` do serviço |
| [Execução](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao/tree/main/infra) | Tópico SNS da Execução, filas de eventos e comandos, DLQs e configuração; manifests `k8s/` do serviço |

Os scripts de `scripts/` coordenam os seis repositórios e a documentação
compartilhada fica aqui.

As antigas cópias de `infra/` e `k8s/`, removidas em 04/09, eram duplicatas da
infraestrutura de rede e do cluster. O `infra/` atual é novo e contém somente
recursos de mensageria próprios do OS Service, conforme a ADR-0005.

Os três tópicos são criados antes das assinaturas para evitar dependência
circular entre os states independentes. O ciclo de vida automatizado faz esse
bootstrap em duas fases; os detalhes estão no
[catálogo de mensageria](mensageria/catalogo.md).

## Subir e derrubar o ambiente na nuvem

Dois comandos, e eles orquestram a infraestrutura compartilhada e os seis
repositórios:

```bash
bash scripts/sobe-tudo.sh
```

```bash
bash scripts/derruba-tudo.sh
```

A ordem das etapas, as dependências entre elas e o que fazer quando dá errado
estão em [CICLO-DE-VIDA.md](CICLO-DE-VIDA.md). Na primeira vez, comece pelo
[roteiro rápido](CICLO-DE-VIDA.md#roteiro-rápido).

> **O cluster cobra sozinho.** O control plane do EKS custa US$ 0,10/hora
> enquanto existir e **não** é suspenso junto com a sessão do Learner Lab.
> Um ambiente de pé custa cerca de US$ 5,40/dia. Derrube ao terminar.

## Desenvolvimento local com kind

**Nenhum pipeline usa kind.** Até 04/09, os workflows `homolog-ci-cd-self-hosted.yml`
e `master-ci-cd-self-hosted.yml` publicavam num cluster kind por um runner
self-hosted Windows. Foram removidos: um runner na máquina de uma pessoa não é
ambiente de homologação nem de produção, e enquanto eles existissem qualquer
promoção de branch acordava pipeline sem runner.

O kind continua útil para testar manifests antes de mandá-los para a nuvem. Um
`kubectl apply` local pega erro de YAML, de probe e de configuração em segundos,
sem cluster EKS cobrando e sem esperar 15 minutos de apply.

A configuração está em
[`local/`](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s/tree/develop/local),
no `infra-k8s`, com o passo a passo no README de lá.

**A diferença que importa entre local e nuvem** está no banco. O kustomization da
raiz de `k8s/` inclui `postgres/` e roda o banco dentro do cluster; o overlay
`k8s/nuvem` **não** inclui, porque na nuvem o banco é o RDS. Aplicar o
kustomization errado na nuvem criaria um segundo banco, vazio, e a API
conversaria com o errado.

## Como a API chega ao usuário

```
cliente
  → API Gateway (HTTP API)          /auth e o webhook do Mercado Pago públicos, /api/v1 atrás do authorizer JWT
  → VPC Link → ALB interno          balanceador em subnet privada, alvos por instância, regra por caminho
      /api/v1/orcamentos*, /api/v1/pagamentos*                       → NodePort 30081 → Billing   → DynamoDB
      /api/v1/catalogo*, /api/v1/diagnosticos*, /api/v1/estoque*, /api/v1/execucoes* → NodePort 30082 → Execução  → RDS próprio
      todo o resto                                                   → NodePort 30080 → OS        → RDS do OS (Multi-AZ)
```

Cada pod lê a connection string e a configuração de mensageria de um secret
do Kubernetes, montado a partir do Secrets Manager a cada deploy. Os bancos
ficam em subnet privada, sem acesso público.

Entre os serviços, a conversa é por mensageria (abaixo), com uma exceção: ao
abrir a OS, o OS consulta o catálogo da Execução por HTTP, direto pelo DNS do
cluster (`http://oficina-mecanica-execucao`), sem passar pelo gateway nem pelo
ALB.

O endereço público é o do API Gateway **com o nome do ambiente no caminho** —
`https://<id>.execute-api.us-east-1.amazonaws.com/dev/auth`. Sem o `/dev`, o
gateway responde `404 {"message":"Not Found"}`, até no `/health/live`.

A autenticação por CPF é uma Lambda fora do cluster, e um segundo Lambda
authorizer valida o JWT na borda do gateway — o desenho e o porquê estão na
[RFC-0002](rfcs/0002-autenticacao-por-cpf-e-api-gateway.md).

## Como os serviços recebem mensagens

Cada serviço provisiona seu tópico e suas filas consumidoras em seu próprio
`infra/`. Cada um grava o topic ARN e as queue URLs num segredo próprio,
`tc-grupo160/<ambiente>/<serviço>/messaging`, e o deploy (pipeline ou
`sobe-tudo.sh`) os passa ao pod como variáveis `Messaging__*`. O contrato e a
topologia estão no [catálogo de eventos](mensageria/catalogo.md).

## Escalabilidade

Duas camadas: o HPA escala **pod** por utilização (2 a 10 réplicas), e o Cluster
Autoscaler escala **node** quando um pod não cabe em lugar nenhum (2 a 4 nodes).

A análise completa, com o teste de carga medido em `dev`, está na
[ADR-0002](adrs/0002-estrategia-de-escalabilidade.md).

## Imagem da API

O `Dockerfile` fica em `docker/backend/`, neste repositório — a imagem é
artefato da aplicação, não da infraestrutura. O deploy a publica no ECR com tag
do SHA do commit, e é essa tag que o rollback usa.

O rollback da aplicação sai no resumo de cada execução do workflow. O rollback do
**banco** é outra coisa, e tem documento próprio:
[ROLLBACK-BANCO.md](ROLLBACK-BANCO.md).
