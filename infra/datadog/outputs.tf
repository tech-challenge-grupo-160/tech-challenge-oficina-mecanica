output "dashboard_url" {
  description = "Caminho do dashboard da Saga no Datadog (somar ao dominio app.datadoghq.com)."
  value       = datadog_dashboard_json.saga.url
}

output "monitor_ids" {
  description = "IDs dos monitores de DLQ e de taxa de compensacao."
  value = {
    dlq_com_mensagens = datadog_monitor.dlq_com_mensagens.id
    taxa_compensacao  = datadog_monitor.taxa_compensacao.id
  }
}

output "metrica_saga" {
  description = "Metrica derivada dos logs com os eventos da Saga, por tipomensagem."
  value       = datadog_logs_metric.saga_eventos.name
}
