# Infraestrutura

Onde cada coisa roda, e onde cada coisa é definida.

> Este documento descrevia, até 04/09, um cluster **kind** local como se fosse a
> infraestrutura do projeto. Não é mais: a infraestrutura é a AWS, e o kind
> sobrou como ambiente de desenvolvimento. Reescrito na
> [#65](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/65).

## Os três ambientes, e o quarto que não é ambiente

| | Onde roda | Banco | Quem publica |
|---|---|---|---|
| `dev`, `hom`, `prod` | EKS, na AWS | RDS PostgreSQL gerenciado | workflow `Deploy EKS` ou `scripts/sobe-tudo.sh` |
| Desenvolvimento local | kind, na sua máquina | PostgreSQL dentro do cluster | você, à mão |
| Só rodar a API | Docker Compose | contêiner do Compose | você, à mão |

A última linha é a que resolve o dia a dia. **Se você só quer a API de pé, use o
Docker Compose** — está no [SETUP.md](SETUP.md) e sobe em segundos. O kind serve
quando o que você está mexendo é o Kubernetes em si.

## Nada disso é definido aqui

Este repositório é a **aplicação**. A infraestrutura vive em outros dois, por
decisão da [ADR-0001](adrs/0001-segregacao-em-quatro-repositorios.md):

| Repositório | O que define |
|---|---|
| [tech-challenge-infra-k8s](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s) | VPC, EKS, API Gateway, ALB, ECR, segredos, manifests do Kubernetes, Cluster Autoscaler — e o `local/`, com o kind |
| [tech-challenge-infra-database](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-database) | RDS PostgreSQL Multi-AZ, subnet group, credencial no Secrets Manager |

O que fica **neste** repositório é o que atravessa os quatro: os scripts de ciclo
de vida em `scripts/`, e a documentação que não pertence a um repositório só.

Até 04/09 havia cópias de `infra/` e `k8s/` aqui, sobras do split. Eram duplicata
estrita do `infra-k8s` e já tinham começado a divergir — foram removidas.

## Subir e derrubar o ambiente na nuvem

Dois comandos, e eles orquestram os quatro repositórios:

```bash
bash scripts/sobe-tudo.sh
```

```bash
bash scripts/derruba-tudo.sh
```

A ordem das etapas, as dependências entre elas e o que fazer quando dá errado
estão em [CICLO-DE-VIDA.md](CICLO-DE-VIDA.md).

> **O cluster cobra sozinho.** O control plane do EKS custa US$ 0,10/hora
> enquanto existir e **não** é suspenso junto com a sessão do Learner Lab.
> Um ambiente de pé custa cerca de US$ 5/dia. Derrube ao terminar.

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
  → API Gateway (HTTP API)          rota /auth pública, /api/v1 atrás do authorizer JWT
  → VPC Link → ALB interno          balanceador em subnet privada, alvos por instância
  → NodePort 30080 nos nodes        kube-proxy encaminha ao pod
  → API .NET                        lê a connection string e a chave do JWT de um Secret
  → RDS PostgreSQL                  Multi-AZ, em subnet privada, sem acesso público
```

A autenticação por CPF é uma Lambda fora do cluster, e um segundo Lambda
authorizer valida o JWT na borda do gateway — o desenho e o porquê estão na
[RFC-0002](rfcs/0002-autenticacao-por-cpf-e-api-gateway.md).

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
