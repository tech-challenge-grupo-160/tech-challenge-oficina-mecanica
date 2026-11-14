data "aws_caller_identity" "current" {}

data "aws_partition" "current" {}

locals {
  service = "os"
  prefix  = "${var.project}-${var.environment}"

  topic_names = {
    os       = "${local.prefix}-os-events"
    billing  = "${local.prefix}-billing-events"
    execucao = "${local.prefix}-execucao-events"
  }

  topic_arns = {
    for service, name in local.topic_names :
    service => "arn:${data.aws_partition.current.partition}:sns:${var.region}:${data.aws_caller_identity.current.account_id}:${name}"
  }

  # Uma fila por tipo de evento, cada uma com DLQ propria (adendo de 11/11 a
  # ADR-0005). E a topologia que o codigo le e que o LocalStack monta
  # (docker/localstack/init-aws.sh): cada consumidor do OS tem a sua fila, que
  # assina o topico do produtor com filtro pelo atributo "tipo" - o nome da
  # classe da mensagem, gravado pelo publicador da Outbox de Billing e
  # Execucao. config_key e o nome que o codigo le em Messaging__<config_key>.
  queues = {
    diagnostico_iniciado = {
      name       = "${local.prefix}-os-diagnostico-iniciado"
      config_key = "DiagnosticoIniciadoQueueUrl"
      producer   = "execucao"
      tipo       = "DiagnosticoIniciadoMessage"
    }
    diagnostico_concluido = {
      name       = "${local.prefix}-os-diagnostico-concluido"
      config_key = "DiagnosticoConcluidoQueueUrl"
      producer   = "execucao"
      tipo       = "DiagnosticoConcluidoMessage"
    }
    pecas_reservadas = {
      name       = "${local.prefix}-os-pecas-reservadas"
      config_key = "PecasReservadasQueueUrl"
      producer   = "execucao"
      tipo       = "PecasReservadasMessage"
    }
    estoque_insuficiente = {
      name       = "${local.prefix}-os-estoque-insuficiente"
      config_key = "EstoqueInsuficienteQueueUrl"
      producer   = "execucao"
      tipo       = "EstoqueInsuficienteMessage"
    }
    execucao_finalizada = {
      name       = "${local.prefix}-os-execucao-finalizada"
      config_key = "ExecucaoFinalizadaQueueUrl"
      producer   = "execucao"
      tipo       = "ExecucaoFinalizadaMessage"
    }
    orcamento_gerado = {
      name       = "${local.prefix}-os-orcamento-gerado"
      config_key = "OrcamentoGeradoQueueUrl"
      producer   = "billing"
      tipo       = "OrcamentoGeradoMessage"
    }
    orcamento_aprovado = {
      name       = "${local.prefix}-os-orcamento-aprovado"
      config_key = "OrcamentoAprovadoQueueUrl"
      producer   = "billing"
      tipo       = "OrcamentoAprovadoMessage"
    }
    orcamento_recusado = {
      name       = "${local.prefix}-os-orcamento-recusado"
      config_key = "OrcamentoRecusadoQueueUrl"
      producer   = "billing"
      tipo       = "OrcamentoRecusadoMessage"
    }
    pagamento_confirmado = {
      name       = "${local.prefix}-os-pagamento-confirmado"
      config_key = "PagamentoConfirmadoQueueUrl"
      producer   = "billing"
      tipo       = "PagamentoConfirmadoMessage"
    }
    # Compensacao de pagamento recusado (F4-27).
    pagamento_recusado = {
      name       = "${local.prefix}-os-pagamento-recusado"
      config_key = "PagamentoRecusadoQueueUrl"
      producer   = "billing"
      tipo       = "PagamentoRecusadoMessage"
    }
  }

  # Filas de comando que o OS Service alimenta por envio direto (SendMessage).
  # Pertencem ao Terraform de Billing e Execucao; aqui a URL e montada pelo
  # nome, como antes, para os states continuarem independentes.
  command_queue_names = {
    ExecucaoCommandQueueUrl           = "${local.prefix}-execucao-iniciar-diagnostico"
    ReservaPecasCommandQueueUrl       = "${local.prefix}-execucao-reservar-pecas"
    IniciarExecucaoCommandQueueUrl    = "${local.prefix}-execucao-iniciar-reparo"
    BillingCommandQueueUrl            = "${local.prefix}-billing-gerar-orcamento"
    SolicitarPagamentoCommandQueueUrl = "${local.prefix}-billing-solicitar-pagamento"
  }

  command_destinations = {
    for config_key, name in local.command_queue_names :
    config_key => {
      name = name
      arn  = "arn:${data.aws_partition.current.partition}:sqs:${var.region}:${data.aws_caller_identity.current.account_id}:${name}"
      url  = "https://sqs.${var.region}.${data.aws_partition.current.dns_suffix}/${data.aws_caller_identity.current.account_id}/${name}"
    }
  }
}

# Hoje o OS Service nao publica eventos (orquestra a Saga so com comandos). O
# topico continua aqui porque os os.* do catalogo nascem nele, e Billing e
# Execucao ja o referenciam.
resource "aws_sns_topic" "events" {
  name              = local.topic_names[local.service]
  kms_master_key_id = "alias/aws/sns"
}

resource "aws_sqs_queue" "dlq" {
  for_each = local.queues

  name                      = "${each.value.name}-dlq"
  message_retention_seconds = var.dlq_retention_seconds
  sqs_managed_sse_enabled   = true
}

resource "aws_sqs_queue" "queue" {
  for_each = local.queues

  name                       = each.value.name
  message_retention_seconds  = var.queue_retention_seconds
  visibility_timeout_seconds = 60
  sqs_managed_sse_enabled    = true
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.dlq[each.key].arn
    maxReceiveCount     = var.max_receive_count
  })
}

resource "aws_sqs_queue_policy" "event_delivery" {
  for_each = local.queues

  queue_url = aws_sqs_queue.queue[each.key].url
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Sid       = "AllowDeliveryFromProducerTopic"
      Effect    = "Allow"
      Principal = { Service = "sns.amazonaws.com" }
      Action    = "sqs:SendMessage"
      Resource  = aws_sqs_queue.queue[each.key].arn
      Condition = {
        ArnEquals = {
          "aws:SourceArn" = local.topic_arns[each.value.producer]
        }
      }
    }]
  })
}

# Filtro pelo atributo "tipo" (MessageAttributes), nao pelo corpo: e onde os
# publicadores gravam o tipo. Com raw_message_delivery o consumidor recebe o
# JSON da mensagem sem o envelope do SNS.
resource "aws_sns_topic_subscription" "events" {
  for_each = local.queues

  topic_arn            = local.topic_arns[each.value.producer]
  protocol             = "sqs"
  endpoint             = aws_sqs_queue.queue[each.key].arn
  raw_message_delivery = true
  filter_policy_scope  = "MessageAttributes"
  filter_policy = jsonencode({
    tipo = [each.value.tipo]
  })

  depends_on = [aws_sqs_queue_policy.event_delivery]
}

resource "aws_secretsmanager_secret" "messaging_config" {
  name                    = "${var.project}/${var.environment}/${local.service}/messaging"
  description             = "Topico, filas e destinos de comando do OS Service."
  recovery_window_in_days = 0
}

# As chaves sao exatamente as que o codigo le (Messaging__*), para o deploy
# copiar o segredo para o pod sem traducao. Messaging__Enabled liga os workers:
# so existe quando a infra existe.
resource "aws_secretsmanager_secret_version" "messaging_config" {
  secret_id = aws_secretsmanager_secret.messaging_config.id
  secret_string = jsonencode(merge(
    {
      Messaging__Enabled = "true"
    },
    {
      for key, queue in local.queues :
      "Messaging__${queue.config_key}" => aws_sqs_queue.queue[key].url
    },
    {
      for config_key, destino in local.command_destinations :
      "Messaging__${config_key}" => destino.url
    }
  ))
}
