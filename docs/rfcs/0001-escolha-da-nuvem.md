# RFC-0001: Escolha da nuvem — AWS

| | |
|---|---|
| **Status** | Em revisão — ambiente e restrições confirmados com o professor em 24/08 |
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
| API Gateway | Amazon API Gateway **HTTP API (v2)**, com Lambda authorizer — ver [emenda](#2026-08-27--api-gateway-http-api-não-rest-api) |
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

**Alternativa adotada, aprovada pelo professor em 24/08:** os pipelines usam as **credenciais temporárias da sessão do lab**, cadastradas como secrets do repositório e renovadas a cada sessão. Isso é pior em segurança e exige passo manual — está registrado como **limitação do ambiente**, não como escolha de arquitetura. O desenho correto (OIDC) está descrito acima para constar na entrega.

**2. Menor privilégio é impossível.** A issue [#59](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/59) previa roles específicas por componente. Com apenas a `LabRole` disponível, Lambda, cluster e pipelines compartilham a mesma identidade — o oposto de menor privilégio. O Terraform deve **referenciar** a role existente via `data`, nunca criar.

> Ao referenciar a `LabRole`, monte o ARN com `data.aws_caller_identity.current.account_id` em vez de escrever o número da conta. Os quatro repositórios são **públicos**.

**3. Sessão de 4 horas.** O ambiente dorme ao fim da sessão. O entregável pede *"Links para os deploys ativos (se aplicável)"* — o professor confirmou em 24/08 que esse item pode ser marcado como **não aplicável**, com a demonstração feita no vídeo. A gravação precisa acontecer com a sessão ativa.

**4. Orçamento de US$ 100 para toda a disciplina, renovável.**

O professor confirmou em 24/08 que **renova o crédito quando acabar**, mediante aviso. Isso remove o custo como fator de decisão de arquitetura — mas não como fator de disciplina operacional.

O control plane do EKS cobra US$ 0,10/hora **enquanto o cluster existir**; ele não é suspenso junto com a sessão do lab, diferente das instâncias EC2:

| Cenário | Custo só do control plane |
|---|---|
| Cluster de pé por 21 dias | US$ 50,40 |
| Criado e destruído a cada sessão de 8h | ~US$ 17 |

Mesmo com renovação disponível, a prática adotada é **`terraform destroy` ao fim de cada sessão de trabalho** — pedir crédito novo por esquecimento é desperdício evitável.

### Decisões decorrentes

- ~~**RDS Single-AZ**, não Multi-AZ. Dobrar o custo do banco não cabe no orçamento.~~ — **revisto em 27/08, ver [emenda](#2026-08-27--rds-multi-az-revertendo-a-decisão-por-single-az)**
- **Sem NAT Gateway**; nodes em subnet pública com security group restritivo. Menos seguro, e registrado como tal. Consequência não antecipada na [emenda do VPC Endpoint](#2026-08-27--vpc-endpoint-de-interface-para-o-secrets-manager).
- **Cluster criado tarde**, próximo à gravação do vídeo, e destruído logo depois. `terraform destroy` ao fim de cada sessão de trabalho.
- **`LabRole` em todos os componentes**, referenciada e nunca criada.

### Alternativa avaliada e descartada: cluster auto-gerenciado

A lista de infraestrutura obrigatória pede "Banco de Dados **Gerenciado**" mas, para Kubernetes, apenas "Cluster Kubernetes **com escalabilidade**" — sem exigir serviço gerenciado. Um cluster k3s ou kubeadm em EC2 atenderia à letra do requisito e eliminaria a cobrança contínua do control plane.

**Descartada.** O único argumento a favor era custo, e com o crédito renovável ele deixou de pesar. O EKS é a resposta esperada na avaliação e evita o risco de interpretação divergente do enunciado.

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
- ~~Existem créditos disponíveis~~ — **resolvido:** US$ 100, renováveis mediante aviso ao professor
- **Quem roda `terraform destroy`** ao fim de cada sessão de trabalho
- ~~EKS ou cluster auto-gerenciado~~ — **resolvido:** EKS, já que o crédito é renovável
- **ALB ou NLB** na entrada do cluster: como o API Gateway já roteia e valida o JWT, parte do que o ALB oferece fica redundante, e o VPC Link do API Gateway exige NLB. Pode ser decidido na issue [#64](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/64) sem bloquear esta RFC

## Decisão

_A preencher ao fechar a RFC._

Sendo aceita, registrar ADR correspondente em [`docs/adrs/`](../adrs/), já que a escolha de nuvem é decisão arquitetural permanente dentro do escopo do projeto.

## Emendas

Decisões tomadas depois da redação original. Ficam registradas aqui em vez de
reescrever o texto acima — saber o que mudou e por quê vale tanto quanto o
estado final.

### 2026-08-27 — RDS Multi-AZ, revertendo a decisão por Single-AZ

A seção "Decisões decorrentes" dizia *"RDS Single-AZ, não Multi-AZ. Dobrar o
custo do banco não cabe no orçamento"* — e **contradizia a tabela de serviços
deste próprio documento**, que já listava Multi-AZ. A contradição existia desde
a redação original e passou despercebida.

Resolvida em favor de Multi-AZ, por dois motivos:

1. **Os números.** `db.t3.micro` em `us-east-1` custa ~US$ 0,018/h Single-AZ
   contra ~US$ 0,036/h Multi-AZ. Com `terraform destroy` ao fim de cada sessão,
   a diferença real é de **~US$ 0,43/dia** — não o "dobrar o custo" que a frase
   sugeria em termos absolutos, contra um orçamento de US$ 100 renovável.
2. **O requisito.** A issue [#61](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/61)
   tem "alta disponibilidade habilitada" como critério de aceite.

Aplicado e verificado em `dev`: instância `tc-grupo160-dev` com `MultiAZ = true`,
criptografada e sem acesso público.

### 2026-08-27 — VPC Endpoint de interface para o Secrets Manager

Consequência não antecipada da decisão de **não ter NAT Gateway**, que continua
válida.

A Lambda de autenticação precisa entrar na VPC para alcançar o RDS em subnet
privada. Ao entrar, perde a saída para a internet — a subnet privada não tem
rota default — e deixaria de ler o segredo do JWT no cold start, quebrando a
issue [#46](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/46).

O endpoint de interface devolve esse acesso por dentro da rede, a
**~US$ 0,01/hora por AZ**, contra ~US$ 0,045/hora do NAT Gateway mais tráfego.
A decisão de não ter NAT segue de pé; o endpoint é o que a torna praticável.

### 2026-08-27 — API Gateway HTTP API, não REST API

A tabela de serviços registrava *"REST API, com JWT authorizer"*. **Essa
combinação não existe na AWS:** o authorizer JWT nativo é exclusivo do HTTP API
(v2), e mesmo lá exige emissor OIDC com JWKS e assinatura assimétrica — que não
é o que a Lambda emite.

Corrigido na tabela acima. A decisão, as alternativas e a justificativa do
Lambda authorizer estão na [RFC-0002](0002-autenticacao-por-cpf-e-api-gateway.md).
