# DLQ com mensagem = um passo da Saga desistiu depois de max_receive_count
# entregas (F4-29). Um alerta por fila (multi-alert por queuename). As metricas
# do SQS chegam pela integracao AWS do Datadog, com atraso de ~10 minutos do
# CloudWatch: por isso a janela de 15 minutos.
resource "datadog_monitor" "dlq_com_mensagens" {
  name = "[${local.env}] DLQ com mensagens: {{queuename.name}}"
  type = "query alert"

  query = "max(last_15m):max:aws.sqs.approximate_number_of_messages_visible{${local.filtro_dlqs}} by {queuename} > 0"

  message = <<-EOT
    {{#is_alert}}
    A DLQ **{{queuename.name}}** tem {{value}} mensagem(ns). Um passo da Saga
    falhou em todas as entregas (maxReceiveCount) e foi desviado para a DLQ.

    Investigue pelo correlationId da mensagem nos logs/APM e reprocesse conforme
    docs/mensageria/catalogo.md.
    {{/is_alert}}
    {{#is_recovery}}
    A DLQ **{{queuename.name}}** foi esvaziada.
    {{/is_recovery}}${local.notificacao}
  EOT

  monitor_thresholds {
    critical = 0
  }

  require_full_window = false
  notify_no_data      = false
  include_tags        = true
  new_group_delay     = 300
  renotify_interval   = 60

  tags = local.tags
}

# Percentual de sagas que terminaram compensadas na ultima hora. Sem nenhuma
# saga na janela a divisao nao tem valor e o monitor fica em No Data, sem
# notificar.
resource "datadog_monitor" "taxa_compensacao" {
  name = "[${local.env}] Taxa de compensacao da Saga da OS acima do esperado"
  type = "query alert"

  query = "sum(last_1h):100 * ${local.consulta_compensadas} / ${local.consulta_desfechos} > ${var.taxa_compensacao_critica}"

  message = <<-EOT
    {{#is_alert}}
    {{value}}% das sagas da OS terminaram compensadas na ultima hora
    (limite: ${var.taxa_compensacao_critica}%). Compensacoes contadas: orcamento
    recusado e pagamento recusado.
    {{/is_alert}}
    {{#is_warning}}
    {{value}}% das sagas da OS terminaram compensadas na ultima hora
    (aviso: ${var.taxa_compensacao_alerta}%).
    {{/is_warning}}
    {{#is_recovery}}
    A taxa de compensacao da Saga voltou ao normal.
    {{/is_recovery}}

    Dashboard: "[${local.env}] Saga da OS - fluxo distribuido".${local.notificacao}
  EOT

  monitor_thresholds {
    critical = var.taxa_compensacao_critica
    warning  = var.taxa_compensacao_alerta
  }

  require_full_window = false
  notify_no_data      = false
  include_tags        = true

  tags = local.tags

  lifecycle {
    precondition {
      condition     = var.taxa_compensacao_alerta < var.taxa_compensacao_critica
      error_message = "taxa_compensacao_alerta deve ser menor que taxa_compensacao_critica."
    }
  }

  depends_on = [datadog_logs_metric.saga_eventos]
}
