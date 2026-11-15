# Trace de ponta a ponta no Datadog (F4-31)

O fluxo de uma OS passa pelos três serviços (OS, Execução e Billing) e pelas
filas SNS/SQS. Para ele aparecer no APM como **um único trace**, o contexto do
trace viaja junto com as mensagens.

## Unified Service Tagging

| Tag | Origem |
|---|---|
| `service` | label `tags.datadoghq.com/service` do pod (`oficina-mecanica-api`, `oficina-mecanica-billing`, `oficina-mecanica-execucao`) |
| `env` | label `tags.datadoghq.com/env`, `__AMBIENTE__` resolvido no deploy (`dev`, `hom`, `prod`); separa os ambientes no APM |
| `version` | label `tags.datadoghq.com/version` = tag da imagem (SHA do commit), copiada por uma `replacement` do Kustomize |

`DD_SERVICE`, `DD_ENV` e `DD_VERSION` são lidos dos labels por `fieldRef`, então
o APM, os logs e as métricas nunca divergem dos labels. Manifests: OS em
`tech-challenge-infra-k8s/k8s/nuvem`; Billing e Execução em `k8s/` de cada
repositório.

## Como o contexto atravessa a Outbox e a fila

```
HTTP POST /ordens-servico (OS)            trace T
 └─ grava OutboxMessage.TraceContext = contexto do span ativo
OutboxSqsPublisherWorker                  span mensageria.publicar (pai = TraceContext)
 └─ SendMessage com atributo _datadog = contexto do span de publicação
SqsConsumerWorker (Execução)              span mensageria.consumir (pai = _datadog)
 └─ grava InboxMessage/OutboxMessage.TraceContext → próxima publicação continua T
...
```

- **Gravar**: o `DbContext` (OS e Execução) e o `DynamoDbMessageItemFactory`
  (Billing) gravam `TraceContext` nas mensagens novas da Outbox e da Inbox.
- **Publicar**: o worker abre `mensageria.publicar` como filho do contexto
  gravado e manda o atributo `_datadog` (JSON com `x-datadog-trace-id`,
  `x-datadog-parent-id`, `x-datadog-tags`, `traceparent`, `tracestate`). É o
  mesmo atributo que o tracer .NET usa na instrumentação automática do AWSSDK;
  se ela também injetar, injeta o contexto do span `aws.sns`/`aws.sqs`, filho
  do nosso, e o trace continua o mesmo.
- **Consumir**: o consumidor pede o atributo `_datadog` no `ReceiveMessage`
  (String no SQS, String ou Binary vindo do SNS com raw message delivery) e
  processa a mensagem dentro de `mensageria.consumir`.

A instrumentação automática sozinha não basta: a Outbox publica num worker sem
span ativo, e o `ReceiveMessage` não deixa o contexto ativo para o
processamento. Código: `Infrastructure/Observability/MensageriaTrace.cs` em
cada serviço.

## Etapas humanas

Concluir diagnóstico, finalizar a execução, aprovar o orçamento e o webhook do
Mercado Pago chegam como requisições HTTP novas, num trace novo. Quando o
serviço já consumiu mensagens da mesma Saga, a publicação usa como pai o
contexto mais recente da Inbox para o `correlationId` e guarda o trace da
requisição na tag `oficina.trace_origem`. A requisição HTTP fica no trace dela;
a Saga continua no trace da OS.

## Como validar

1. Abrir uma OS em `dev` e seguir a Saga até o fim (ou até onde o teste for).
2. APM → Traces, filtrar `env:dev @oficina.ordem_servico_id:<número da OS>`
   (ou `@oficina.correlation_id:<id>`).
3. Abrir o trace: devem aparecer os três serviços, com spans
   `mensageria.publicar` / `mensageria.consumir` por tipo de mensagem
   (resource = tipo, ex.: `IniciarDiagnosticoMessage`).
4. Num span de etapa humana, `oficina.trace_origem` leva ao trace da
   requisição HTTP que a disparou.
5. Service Map (`env:dev`): arestas OS → Execução e OS → Billing.

Mensagens gravadas antes do deploy não têm `TraceContext`: uma OS que já estava
em andamento começa um trace novo a partir da próxima mensagem.
