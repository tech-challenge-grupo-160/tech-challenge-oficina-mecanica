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

  queues = {
    billing_events = {
      name     = "${local.prefix}-os-billing-eventos"
      producer = "billing"
      message_types = [
        "billing.orcamento-enviado",
        "billing.orcamento-aprovado",
        "billing.orcamento-recusado",
        "billing.pagamento-confirmado",
        "billing.pagamento-recusado",
      ]
    }
    execution_events = {
      name     = "${local.prefix}-os-execucao-eventos"
      producer = "execucao"
      message_types = [
        "execucao.diagnostico-concluido",
        "execucao.pecas-reservadas",
        "execucao.estoque-insuficiente",
        "execucao.finalizada",
        "execucao.abortada",
      ]
    }
  }

  command_destinations = {
    for service in ["billing", "execucao"] :
    service => {
      name = "${local.prefix}-${service}-comandos"
      arn  = "arn:${data.aws_partition.current.partition}:sqs:${var.region}:${data.aws_caller_identity.current.account_id}:${local.prefix}-${service}-comandos"
      url  = "https://sqs.${var.region}.${data.aws_partition.current.dns_suffix}/${data.aws_caller_identity.current.account_id}/${local.prefix}-${service}-comandos"
    }
  }
}

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

resource "aws_sns_topic_subscription" "events" {
  for_each = local.queues

  topic_arn            = local.topic_arns[each.value.producer]
  protocol             = "sqs"
  endpoint             = aws_sqs_queue.queue[each.key].arn
  raw_message_delivery = true
  filter_policy_scope  = "MessageBody"
  filter_policy = jsonencode({
    type = each.value.message_types
  })

  depends_on = [aws_sqs_queue_policy.event_delivery]
}

resource "aws_secretsmanager_secret" "messaging_config" {
  name                    = "${var.project}/${var.environment}/${local.service}/messaging"
  description             = "Topico, filas e destinos de comando do OS Service."
  recovery_window_in_days = 0
}

resource "aws_secretsmanager_secret_version" "messaging_config" {
  secret_id = aws_secretsmanager_secret.messaging_config.id
  secret_string = jsonencode({
    Messaging__EventTopicName             = aws_sns_topic.events.name
    Messaging__EventTopicArn              = aws_sns_topic.events.arn
    Messaging__BillingEventsQueueName     = aws_sqs_queue.queue["billing_events"].name
    Messaging__BillingEventsQueueUrl      = aws_sqs_queue.queue["billing_events"].url
    Messaging__BillingEventsQueueArn      = aws_sqs_queue.queue["billing_events"].arn
    Messaging__BillingEventsDlqArn        = aws_sqs_queue.dlq["billing_events"].arn
    Messaging__ExecutionEventsQueueName   = aws_sqs_queue.queue["execution_events"].name
    Messaging__ExecutionEventsQueueUrl    = aws_sqs_queue.queue["execution_events"].url
    Messaging__ExecutionEventsQueueArn    = aws_sqs_queue.queue["execution_events"].arn
    Messaging__ExecutionEventsDlqArn      = aws_sqs_queue.dlq["execution_events"].arn
    Messaging__BillingCommandsQueueName   = local.command_destinations.billing.name
    Messaging__BillingCommandsQueueUrl    = local.command_destinations.billing.url
    Messaging__BillingCommandsQueueArn    = local.command_destinations.billing.arn
    Messaging__ExecutionCommandsQueueName = local.command_destinations.execucao.name
    Messaging__ExecutionCommandsQueueUrl  = local.command_destinations.execucao.url
    Messaging__ExecutionCommandsQueueArn  = local.command_destinations.execucao.arn
  })
}
