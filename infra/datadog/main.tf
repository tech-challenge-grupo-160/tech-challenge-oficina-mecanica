locals {
  env = var.environment

  # Filas e DLQs dos tres servicos seguem <project>-<ambiente>-<nome>[-dlq]
  # (infra/messaging.tf daqui, de Billing e de Execucao). A integracao AWS do
  # Datadog publica o nome em minusculas na tag queuename.
  filtro_filas = "queuename:${var.project}-${local.env}-*,!queuename:*-dlq"
  filtro_dlqs  = "queuename:${var.project}-${local.env}-*-dlq"

  filtro_os = "service:${var.servico_os},env:${local.env}"

  # Metrica derivada dos logs do OS Service. Leva o ambiente no nome porque
  # metricas de log sao globais na organizacao: dev, hom e prod tem cada uma a
  # sua, criada pelo apply do proprio ambiente.
  metrica_saga = "oficina_mecanica.saga.eventos.${local.env}"

  # Desfechos da Saga (ADR-0004). Os valores sao o nome da classe da mensagem,
  # em minusculas, como o Datadog grava a tag.
  # - Concluida: pagamento confirmado, ultimo passo assincrono (a entrega e HTTP).
  # - Compensada: orcamento recusado (OS cancelada) e pagamento recusado
  #   (entrega bloqueada ate nova cobranca).
  tipos_concluidas  = ["pagamentoconfirmadomessage"]
  tipos_compensadas = ["orcamentorecusadomessage", "pagamentorecusadomessage"]

  consulta_concluidas  = "sum:${local.metrica_saga}{tipo_mensagem IN (${join(",", local.tipos_concluidas)})}.as_count()"
  consulta_compensadas = "sum:${local.metrica_saga}{tipo_mensagem IN (${join(",", local.tipos_compensadas)})}.as_count()"
  consulta_desfechos   = "sum:${local.metrica_saga}{tipo_mensagem IN (${join(",", concat(local.tipos_concluidas, local.tipos_compensadas))})}.as_count()"

  servicos_apm = join(",", var.servicos_apm)

  notificacao = length(var.notificar) > 0 ? "\n\nNotificar: ${join(" ", var.notificar)}" : ""

  tags = [
    "env:${local.env}",
    "service:${var.servico_os}",
    "dashboard:saga-os",
    "managed-by:terraform",
  ]
}

# Cada mensagem que o OS consome com sucesso gera o log
# "Mensagem <Tipo> processada." (SqsConsumerWorker), com o atributo
# TipoMensagem. Contar esses logs por tipo da o ritmo de cada passo da Saga e
# os desfechos, sem instrumentar o codigo. So conta a partir da criacao da
# metrica: o Datadog nao reprocessa logs antigos.
resource "datadog_logs_metric" "saga_eventos" {
  name = local.metrica_saga

  compute {
    aggregation_type = "count"
  }

  filter {
    query = "service:${var.servico_os} env:${local.env} @TipoMensagem:* \"processada\""
  }

  group_by {
    path     = "@TipoMensagem"
    tag_name = "tipo_mensagem"
  }
}

resource "datadog_dashboard_json" "saga" {
  dashboard = templatefile("${path.module}/dashboards/saga-os.json.tftpl", {
    env                  = local.env
    filtro_os            = local.filtro_os
    filtro_filas         = local.filtro_filas
    filtro_dlqs          = local.filtro_dlqs
    metrica_saga         = local.metrica_saga
    consulta_concluidas  = local.consulta_concluidas
    consulta_compensadas = local.consulta_compensadas
    consulta_desfechos   = local.consulta_desfechos
    servicos_apm         = local.servicos_apm
    taxa_critica         = var.taxa_compensacao_critica
    taxa_alerta          = var.taxa_compensacao_alerta
  })

  depends_on = [datadog_logs_metric.saga_eventos]
}
