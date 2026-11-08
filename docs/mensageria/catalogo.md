# Catálogo de mensagens e infraestrutura

Catálogo operacional da [ADR-0005](../adrs/0005-mensageria-sns-sqs.md). O
envelope segue o formato acordado na ADR: `messageId`, `type`, `version`,
`correlationId` (id da OS), `causationId`, `occurredAt` e `producer`.

## Eventos SNS

Cada produtor publica os eventos em seu próprio tópico. Filas independentes
por consumidor assinam o tópico com filtro SNS pelo campo `type` do envelope.

| Tipo | Produtor | Consumidores |
|---|---|---|
| `os.aberta` | OS Service | Billing, Execução |
| `os.cancelada` | OS Service | Billing, Execução |
| `billing.orcamento-enviado` | Billing | OS Service |
| `billing.orcamento-aprovado` | Billing | OS Service |
| `billing.orcamento-recusado` | Billing | OS Service |
| `billing.pagamento-confirmado` | Billing | OS Service |
| `billing.pagamento-recusado` | Billing | OS Service |
| `execucao.diagnostico-concluido` | Execução | OS Service |
| `execucao.pecas-reservadas` | Execução | OS Service |
| `execucao.estoque-insuficiente` | Execução | OS Service |
| `execucao.finalizada` | Execução | OS Service |
| `execucao.abortada` | Execução | OS Service |

| Produtor | Tópico por ambiente |
|---|---|
| OS Service | `tc-grupo160-<ambiente>-os-events` |
| Billing | `tc-grupo160-<ambiente>-billing-events` |
| Execução | `tc-grupo160-<ambiente>-execucao-events` |

## Comandos SQS

Comandos são enviados diretamente para a fila do serviço de destino, sem
passarem por tópico SNS.

| Tipo | Produtor | Fila consumidora |
|---|---|---|
| `execucao.solicitar-diagnostico` | OS Service | `...-execucao-comandos` |
| `execucao.reservar-pecas` | OS Service | `...-execucao-comandos` |
| `execucao.liberar-reservas` | OS Service | `...-execucao-comandos` |
| `billing.gerar-orcamento` | OS Service | `...-billing-comandos` |
| `billing.cobrar` | OS Service | `...-billing-comandos` |
| `billing.cancelar-orcamento` | OS Service | `...-billing-comandos` |

As filas de comandos não têm assinatura SNS; o campo `type` roteia o comando
dentro do serviço consumidor.

## Filas, filtros e segurança

| Serviço dono da fila | Fila | Origem | Filtro |
|---|---|---|---|
| Billing | `...-billing-os-eventos` | Tópico OS | `os.aberta`, `os.cancelada` |
| Billing | `...-billing-comandos` | Envio direto do OS Service | — |
| Execução | `...-execucao-os-eventos` | Tópico OS | `os.aberta`, `os.cancelada` |
| Execução | `...-execucao-comandos` | Envio direto do OS Service | — |
| OS Service | `...-os-billing-eventos` | Tópico Billing | Todos os cinco eventos Billing |
| OS Service | `...-os-execucao-eventos` | Tópico Execução | Todos os cinco eventos Execução |

Cada fila tem uma DLQ exclusiva e redrive após cinco recebimentos. Filas
principais retêm mensagens por quatro dias; DLQs, por 14 dias. SNS usa a chave
gerenciada pela AWS para SNS e SQS usa criptografia gerenciada pelo serviço.

Os Terraform roots responsáveis por tópico e filas são:

- [OS Service](../../infra/README.md)
- [Billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing/tree/main/infra)
- [Execução](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao/tree/main/infra)

Cada serviço grava sua configuração em
`tc-grupo160/<ambiente>/<serviço>/messaging`, no Secrets Manager. O JSON
contém chaves `Messaging__*` (compatíveis com a configuração .NET por
variáveis de ambiente) com nome/ARN do tópico, nomes/URLs/ARNs das filas e,
no OS Service, os destinos das filas de comando. O workflow de deploy do OS
Service injeta esses valores no `api-secret`; Billing e Execução devem usar o
mesmo mapeamento ao habilitar seus deploys independentes.

## Primeiro provisionamento

Como cada produtor e consumidor tem seu próprio state, a criação inicial exige
duas fases:

1. Criar apenas `aws_sns_topic.events` nos três serviços.
2. Executar o apply completo em cada serviço, para que as assinaturas possam
   apontar para tópicos que já existem.

Depois dessa inicialização, o workflow Terraform de cada repositório executa
plan nos PRs e apply após push nas branches de ambiente (`develop`, `homolog`
e `master` no OS Service; `develop`, `homolog` e `main` em Billing e Execução).
O apply requer a variável de repositório `TF_STATE_BUCKET`, opcionalmente
`TF_LOCK_TABLE` e `AWS_REGION`, e os secrets `AWS_ACCESS_KEY_ID`,
`AWS_SECRET_ACCESS_KEY` e `AWS_SESSION_TOKEN`.

### Teste direto na AWS Academy Learner Lab

A validação registrada abaixo foi feita diretamente no Learner Lab, seguindo
este catálogo, sem subir EKS ou RDS.

Use uma sessão Learner Lab ativa e um perfil AWS local com as credenciais
temporárias da sessão. Não registre chaves, session token, state Terraform ou
IDs de conta no repositório.

```powershell
$env:AWS_PROFILE = "tc-lab"
$env:AWS_REGION = "us-east-1"
aws sts get-caller-identity
```

Inicialize cada root sem backend S3 para manter esse teste isolado dos states
remotos usados pelas pipelines:

```powershell
$roots = @(
  'C:\caminho\tech-challenge-oficina-mecanica\infra',
  'C:\caminho\tech-challenge-oficina-mecanica-billing\infra',
  'C:\caminho\tech-challenge-oficina-mecanica-execucao\infra'
)

foreach ($root in $roots) {
  Push-Location $root
  terraform init -backend=false
  if ($LASTEXITCODE -ne 0) { Pop-Location; throw "terraform init falhou: $root" }

  terraform plan -var="environment=dev" -var="region=us-east-1"
  if ($LASTEXITCODE -ne 0) { Pop-Location; throw "terraform plan falhou: $root" }
  Pop-Location
}
```

No primeiro apply de uma conta, crie somente o tópico de cada root antes de
criar assinaturas, pois os roots pertencem a states separados:

```powershell
foreach ($root in $roots) {
  Push-Location $root
  terraform apply -target=aws_sns_topic.events `
    -var="environment=dev" -var="region=us-east-1"
  if ($LASTEXITCODE -ne 0) { Pop-Location; throw "criacao do topico falhou: $root" }
  Pop-Location
}
```

Com os três tópicos criados, aplique a topologia completa nos três roots:

```powershell
foreach ($root in $roots) {
  Push-Location $root
  terraform apply -var="environment=dev" -var="region=us-east-1"
  if ($LASTEXITCODE -ne 0) { Pop-Location; throw "apply falhou: $root" }
  Pop-Location
}
```

Revise o plan e confirme o ambiente `dev` antes de aceitar o apply. Se a conta
já tiver recursos criados fora do state daquele root, pare: não repita o apply
nem misture state local e remoto. Primeiro reconcilie o state com os recursos
existentes. Não execute `scripts/sobe-tudo.sh` depois de um apply com state
local sem reconciliar, pois esse script inicializa o backend remoto S3.

#### Conferir recursos

Confirme os nomes das filas criadas:

```powershell
aws sqs list-queues --region us-east-1 `
  --queue-name-prefix tc-grupo160-dev- `
  --query 'QueueUrls' --output table
```

Confirme os quatro caminhos SNS → SQS. As duas filas de comando são enviadas
diretamente pelo produtor e, intencionalmente, não aparecem como assinaturas:

```powershell
aws sns list-subscriptions --region us-east-1 `
  --query 'Subscriptions[?Protocol==`sqs`].[TopicArn,Endpoint]' --output table
```

Confira filtros, escopo do filtro e entrega raw para cada assinatura:

```powershell
$subscriptions = aws sns list-subscriptions --region us-east-1 --output json |
  ConvertFrom-Json

foreach ($subscription in $subscriptions.Subscriptions | Where-Object Protocol -eq 'sqs') {
  aws sns get-subscription-attributes --region us-east-1 `
    --subscription-arn $subscription.SubscriptionArn `
    --query 'Attributes.{Topic:TopicArn,Queue:Endpoint,Filter:FilterPolicy,FilterScope:FilterPolicyScope,Raw:RawMessageDelivery}' `
    --output table
}
```

Confira redrive para DLQ, retenção e criptografia de todas as filas:

```powershell
$queueUrls = aws sqs list-queues --region us-east-1 `
  --queue-name-prefix tc-grupo160-dev- `
  --query 'QueueUrls' --output json | ConvertFrom-Json

foreach ($url in $queueUrls) {
  aws sqs get-queue-attributes --region us-east-1 --queue-url $url `
    --attribute-names All `
    --query 'Attributes.{Queue:QueueArn,Retention:MessageRetentionPeriod,SSE:SqsManagedSseEnabled,Redrive:RedrivePolicy}' `
    --output table
}
```

#### Resultado registrado

Na validação manual do ambiente `dev` em 08/11/2026, a saída fornecida mostrou:

- 6 filas de consumo/comando e 6 DLQs (12 filas no total);
- 4 assinaturas SNS para as filas de eventos;
- tópico OS assinado pelas filas de eventos do Billing e da Execução;
- tópicos Billing e Execução assinados pela fila correspondente do OS Service;
- outputs Terraform com nomes, ARNs, URLs e nomes dos secrets.

Isso confirma a existência da topologia e suas ligações. A saída compartilhada
não mostrou os atributos das assinaturas nem o redrive/encryption das filas;
rode os comandos acima para validar esses critérios. Também não comprova que
pods EKS consigam acessar SNS/SQS com a LabRole: essa prova de acesso em runtime
continua pertencendo ao spike F4-05.

### Subir o ambiente completo

Depois que os states dos três roots de mensageria estiverem no backend remoto,
o script de ciclo de vida os aplica junto com rede, cluster, banco e aplicações:

```bash
bash scripts/sobe-tudo.sh --ambiente dev
```

No Windows, execute pelo Git Bash. Esse comando provisiona recursos AWS que
podem gerar cobrança contínua (incluindo EKS, NAT e RDS); confira a confirmação
interativa e o custo antes de prosseguir. Para encerrar o ambiente:

```bash
bash scripts/derruba-tudo.sh --ambiente dev
```

Não use o ciclo de vida completo para validar apenas SNS/SQS. O `sobe-tudo.sh`
usa states remotos S3 e, antes de executá-lo, confirme que os recursos da
mensageria estão registrados nos states remotos `dev/messaging-*.tfstate`.
