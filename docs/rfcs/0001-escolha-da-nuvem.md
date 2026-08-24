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
| Região | `sa-east-1` (São Paulo) |

A arquitetura resultante está desenhada em [`docs/diagrams/C4_04_AWS_Deployment_Diagram.puml`](../diagrams/C4_04_AWS_Deployment_Diagram.puml).

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

**Custo:** o EKS cobra US$ 0,10/hora pelo control plane (~US$ 72/mês) e **não tem camada gratuita**. É o item dominante. Somados nodes t3.medium, RDS db.t3.micro Multi-AZ, NAT Gateway e ALB, a ordem de grandeza é de **algumas dezenas a pouco mais de cem dólares por mês**.

> ⚠️ **Este número precisa ser confirmado antes do `apply`.** Não foi validado no Pricing Calculator, e o grupo deve verificar qual conta será usada e se há créditos disponíveis. Um cluster EKS esquecido ligado após a entrega continua cobrando.

**Segurança:** RDS e Lambda em subnets privadas; segredos no Secrets Manager; pipelines autenticando por OIDC, sem chave estática.

## Questões em aberto

- **Qual conta AWS será usada** e quem responde pela fatura
- **Existem créditos** de estudante ou promocionais disponíveis
- **Quem destrói a infraestrutura** após a entrega, e em que data
- **ALB ou NLB** na entrada do cluster: como o API Gateway já roteia e valida o JWT, parte do que o ALB oferece fica redundante, e o VPC Link do API Gateway exige NLB. Pode ser decidido na issue [#64](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/64) sem bloquear esta RFC

## Decisão

_A preencher ao fechar a RFC._

Sendo aceita, registrar ADR correspondente em [`docs/adrs/`](../adrs/), já que a escolha de nuvem é decisão arquitetural permanente dentro do escopo do projeto.
