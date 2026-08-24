# RFC-0001: Escolha da nuvem — AWS

| | |
|---|---|
| **Status** | Em revisão |
| **Autor** | Grupo 160 |
| **Data** | 2026-08-24 |
| **Issue** | [#56](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/56) |
| **Prazo para comentários** | 2026-08-27 |

## Resumo

Propõe-se adotar a **AWS** como provedor de nuvem para a Fase 3, usando API Gateway, Lambda, EKS e RDS PostgreSQL.

Esta RFC tem uma particularidade que precisa ser dita de forma direta: **a decisão já foi tomada na prática**. Desde 17/08 o repositório `tech-challenge-lambda-auth` contém `aws-lambda-tools-defaults.json`, com `function-runtime: dotnet10` e handler configurado para o AWS Lambda. O documento formaliza e justifica uma escolha que o código já expressa, em vez de fingir que a discussão está aberta.

Isso não é ideal — decisão de nuvem deveria preceder a implementação. Mas é preferível registrar com honestidade a inverter a ordem e escrever uma justificativa fabricada.

## Motivação

A Fase 3 exige quatro serviços de nuvem, com liberdade de escolha de provedor:

- API Gateway para controle e roteamento
- Function Serverless para autenticação
- Banco de Dados Gerenciado
- Cluster Kubernetes com escalabilidade

Sem a decisão registrada, nenhum Terraform pode ser escrito — e hoje **10 issues do épico E3 estão paradas**, bloqueando outras 24 de forma transitiva. Cerca de 65% do backlog aberto depende deste documento.

## Proposta

| Necessidade | Serviço AWS |
|---|---|
| API Gateway | Amazon API Gateway (REST API, com JWT authorizer) |
| Function Serverless | AWS Lambda (`dotnet10`, x86_64, 512 MB, timeout 30 s) |
| Banco gerenciado | Amazon RDS PostgreSQL, Multi-AZ, em subnet privada |
| Cluster Kubernetes | Amazon EKS com node group gerenciado e autoscaling |
| Registro de imagens | Amazon ECR |
| Segredos | AWS Secrets Manager |
| IaC | Terraform, state remoto em S3 com lock em DynamoDB |
| Região | `us-east-1` (imposta pelo ambiente) |

A arquitetura resultante está desenhada em [`docs/diagrams/C4_04_AWS_Deployment_Diagram.puml`](../diagrams/C4_04_AWS_Deployment_Diagram.puml).

## Restrições do ambiente: AWS Academy Learner Lab

A conta usada é um **AWS Academy Learner Lab**, e isso impõe limites que alteram a arquitetura. Verificado na conta em 24/08/2026:

| Verificação | Resultado |
|---|---|
| Região | `us-east-1`, sem escolha |
| Identidade | `assumed-role/voclabs/...` — role temporária de sessão |
| `iam:CreateOpenIDConnectProvider` | **AccessDenied** |
| `LabRole` | existe e é a única role utilizável |

### Consequências

**1. OIDC para os pipelines é inviável.** A issue [#54](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/54) previa federação de identidade entre GitHub Actions e AWS, sem chave estática. O ambiente bloqueia a criação do provedor OIDC e de roles.

**Alternativa adotada:** os pipelines usam as **credenciais temporárias da sessão do lab**, cadastradas como secrets do repositório e renovadas a cada sessão. Isso é pior em segurança e exige passo manual — está registrado como **limitação conhecida do ambiente**, não como escolha de arquitetura.

**2. Menor privilégio é impossível.** A issue [#59](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/59) previa roles específicas por componente. Com apenas a `LabRole` disponível, Lambda, cluster e pipelines compartilham a mesma identidade — o oposto de menor privilégio. O Terraform deve **referenciar** a role existente via `data`, nunca criar.

> Ao referenciar a `LabRole`, monte o ARN com `data.aws_caller_identity.current.account_id` em vez de escrever o número da conta. Os quatro repositórios são **públicos**.

**3. Sessão de 4 horas.** O ambiente dorme ao fim da sessão. O entregável pede *"Links para os deploys ativos"* — esses links estarão fora do ar fora da janela de uso. A gravação do vídeo precisa acontecer com a sessão ativa, e a limitação deve ser declarada na entrega.

**4. Orçamento de US$ 100 para toda a disciplina.**

O control plane do EKS cobra US$ 0,10/hora **enquanto o cluster existir** — ele não é suspenso junto com a sessão, diferente das instâncias EC2:

| Cenário | Custo só do control plane |
|---|---|
| Cluster de pé por 21 dias | **US$ 50,40** — metade do crédito |
| Criado e destruído a cada sessão de 8h | ~US$ 17 |

Somados nodes, RDS, NAT Gateway (~US$ 32/mês) e ALB, **deixar o cluster ligado esgota o crédito antes da entrega**.

### Decisões decorrentes

- **RDS Single-AZ**, não Multi-AZ. Dobrar o custo do banco não cabe no orçamento.
- **Sem NAT Gateway**; nodes em subnet pública com security group restritivo. Menos seguro, e registrado como tal.
- **Cluster criado tarde**, próximo à gravação do vídeo, e destruído logo depois. `terraform destroy` ao fim de cada sessão de trabalho.
- **`LabRole` em todos os componentes**, referenciada e nunca criada.

### Uma alternativa que o enunciado permite

A lista de infraestrutura obrigatória pede "Banco de Dados **Gerenciado**" mas, para Kubernetes, apenas "Cluster Kubernetes **com escalabilidade**" — sem exigir que seja gerenciado.

Um cluster auto-gerenciado (k3s ou kubeadm) em EC2, com HPA e autoscaling de nodes, atenderia à letra do requisito, rodaria em instâncias que o lab suspende junto com a sessão e **eliminaria os US$ 0,10/hora contínuos**.

É uma leitura defensável do enunciado, mas é uma aposta: EKS é a resposta que o avaliador provavelmente espera. **Recomenda-se confirmar com o professor antes de decidir.**

## Alternativas avaliadas

| Critério | AWS | Azure | GCP |
|---|---|---|---|
| Serviços obrigatórios cobertos | 4/4 | 4/4 | 4/4 |
| Integração nativa .NET Lambda | Madura (`Amazon.Lambda.*`) | Madura (Functions) | Cloud Functions com suporte .NET menos direto |
| Familiaridade do time | Já em uso no código | Nenhuma | Nenhuma |
| Custo estimado no período | Ver seção abaixo | Comparável | Comparável |
| Retrabalho se adotada agora | Zero | Reescrever a Lambda e o `.csproj` | Reescrever a Lambda e o `.csproj` |

**Azure** e **GCP** atendem tecnicamente aos quatro requisitos. Foram descartadas por um motivo prático e não técnico: a Lambda já está escrita contra a API do AWS Lambda, e faltam 21 dias para a entrega. Migrar consumiria tempo do caminho crítico — que já está atrasado — sem ganho perceptível na avaliação.

Se esta RFC estivesse sendo escrita antes da implementação, a comparação mereceria mais peso. Registrar isso é parte da honestidade do documento.

## Impacto

**Repositórios:** `tech-challenge-infra-k8s` e `tech-challenge-infra-database` passam a usar o provider `aws` do Terraform. `tech-challenge-lambda-auth` mantém o que já existe.

**Cronograma:** destrava [#57](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/57), [#58](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/58), [#59](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/59), [#60](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/60) e [#61](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/61), e por consequência o épico de observabilidade inteiro.

**Custo:** ver a seção de restrições do ambiente. Orçamento total de US$ 100, com o control plane do EKS consumindo até metade dele se o cluster ficar de pé durante a fase.

> ⚠️ **Confirmar no Pricing Calculator antes do primeiro `apply`.** Um cluster EKS esquecido ligado continua cobrando mesmo com a sessão do lab encerrada.

**Segurança:** RDS e Lambda em subnets privadas; segredos no Secrets Manager; pipelines autenticando por OIDC, sem chave estática.

## Questões em aberto

- ~~Qual conta AWS será usada~~ — **resolvido:** AWS Academy Learner Lab, `us-east-1`
- ~~Existem créditos disponíveis~~ — **resolvido:** US$ 100 para toda a disciplina
- **Quem roda `terraform destroy`** ao fim de cada sessão, para o crédito não vazar
- **EKS ou cluster auto-gerenciado** — decisão de custo descrita acima, vale confirmar com o professor
- **ALB ou NLB** na entrada do cluster: como o API Gateway já roteia e valida o JWT, parte do que o ALB oferece fica redundante, e o VPC Link do API Gateway exige NLB. Pode ser decidido na issue [#64](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/64) sem bloquear esta RFC

## Decisão

_A preencher ao fechar a RFC._

Sendo aceita, registrar ADR correspondente em [`docs/adrs/`](../adrs/), já que a escolha de nuvem é decisão arquitetural permanente dentro do escopo do projeto.
