# Dashboard e monitores da Saga no Datadog (F4-37)

Este diretório versiona no Terraform o que se acompanha da Saga da OS no
Datadog. O state é separado do da mensageria (`infra/`): um erro no Datadog não
trava o apply das filas.

## Recursos

| Recurso | O que é |
|---|---|
| `datadog_logs_metric.saga_eventos` | Métrica `oficina_mecanica.saga.eventos.<ambiente>`, contada a partir do log `Mensagem <Tipo> processada.` do `SqsConsumerWorker`, com a tag `tipo_mensagem` |
| `datadog_dashboard_json.saga` | Dashboard `[<ambiente>] Saga da OS - fluxo distribuido`, definido em [`dashboards/saga-os.json.tftpl`](dashboards/saga-os.json.tftpl) |
| `datadog_monitor.dlq_com_mensagens` | Alerta por fila quando uma DLQ do ambiente tem mensagem |
| `datadog_monitor.taxa_compensacao` | Alerta quando o percentual de sagas compensadas na última hora passa do limite (padrão: aviso em 15%, crítico em 30%) |

### Painéis do dashboard

| Grupo | Fonte |
|---|---|
| Saga: concluídas x compensadas, taxa de compensação, OS abertas | Métrica de log `saga.eventos` e `oficina_mecanica.orders.created` (DogStatsD) |
| OS por status (eventos da saga) | Métrica de log `saga.eventos`, por `tipo_mensagem` |
| Latência por passo | `oficina_mecanica.orders.stage_duration` (p50/p95 por `stage`) e APM `trace.aspnet_core.request` dos três serviços |
| Filas e DLQs | Integração AWS: `aws.sqs.approximate_number_of_messages_visible` e `approximate_age_of_oldest_message`, filtradas por `tc-grupo160-<ambiente>-*` |
| Alertas | Status dos monitores com a tag `dashboard:saga-os` |

### Como a Saga é contada

Não há instrumentação nova no código. Os desfechos saem do tipo da mensagem
que o OS Service processou com sucesso (ADR-0004):

- **Concluída:** `PagamentoConfirmadoMessage`, o último passo assíncrono. A
  entrega (`Entregue`) é uma requisição HTTP e fica de fora.
- **Compensada:** `OrcamentoRecusadoMessage` (OS cancelada) e
  `PagamentoRecusadoMessage` (entrega bloqueada até uma nova cobrança).

Limitações:

- A métrica de log só conta a partir da criação. O Datadog não reprocessa logs
  antigos.
- "OS por status" mostra as transições (eventos processados por tipo), não um
  retrato de quantas OS estão hoje em cada status. Um retrato exigiria emitir
  um gauge a partir do código ou do banco.
- Um pagamento recusado seguido de um confirmado conta nos dois desfechos.
- O cancelamento manual pela API não entra como compensação.

## Pré-requisitos

1. **Application Key.** A API de dashboards e monitores exige uma Application
   Key, além da API key. Crie a sua em *Organization Settings → Application
   Keys* (<https://app.datadoghq.com/organization-settings/application-keys>) e
   carregue-a sem gravar no histórico:

   ```bash
   read -rsp "Application Key do Datadog: " TF_VAR_datadog_app_key && echo && export TF_VAR_datadog_app_key
   ```

   A API key é a mesma do Agent (`TF_VAR_datadog_api_key`, ver
   [Chave do Datadog](../../docs/CICLO-DE-VIDA.md#chave-do-datadog)).

2. **Integração AWS no Datadog.** Os painéis de filas e o monitor de DLQ
   dependem das métricas `aws.sqs.*`, que chegam pela integração AWS
   (*Integrations → Amazon Web Services*). Sem ela, esses painéis ficam vazios
   e o monitor de DLQ fica em *No Data*.

3. **Logs e APM do OS Service no Datadog.** Para isso, o Agent precisa estar
   no EKS com `logs.containerCollectAll` e o deploy precisa do unified service
   tagging (`service:oficina-mecanica-api`, `env:<ambiente>`). O tagging entra
   com a F4-31.

## Aplicação

Use o mesmo bucket de state dos outros Terraforms, com uma chave própria:

```bash
terraform -chdir=infra/datadog init -reconfigure \
  -backend-config="bucket=tc-grupo160-tfstate-<id-da-conta>" \
  -backend-config="key=dev/datadog-os.tfstate" \
  -backend-config="region=us-east-1" \
  -backend-config="dynamodb_table=tc-grupo160-tflock"

terraform -chdir=infra/datadog plan -var="environment=dev" \
  -var='notificar=["@seu-email@exemplo.com"]'
```

Revise o plan e depois rode `apply` com as mesmas variáveis. A URL do dashboard
sai no output `dashboard_url`.

Variáveis opcionais: `notificar` (destinos dos alertas; vazio, o monitor só
muda de estado), `taxa_compensacao_critica`, `taxa_compensacao_alerta`,
`servico_os`, `servicos_apm` e `datadog_api_url`.

## Alterando o dashboard

O arquivo versionado é a fonte da verdade: o próximo `apply` sobrescreve o que
for mudado na interface. Para ajustar o visual, edite o dashboard no Datadog,
exporte o JSON (*Settings → Export dashboard JSON*), leve os widgets para
`dashboards/saga-os.json.tftpl` e troque o ambiente e as consultas pelas
variáveis `${...}` do template antes de abrir o PR.
