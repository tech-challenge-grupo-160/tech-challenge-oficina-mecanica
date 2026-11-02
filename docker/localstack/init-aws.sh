#!/bin/bash
set -euo pipefail

REGION="us-east-1"
ACCOUNT_ID="000000000000"

EXECUCAO_DLQ_URL=$(awslocal sqs create-queue \
  --queue-name execucao-iniciar-diagnostico-dlq \
  --query QueueUrl --output text)
EXECUCAO_DLQ_ARN=$(awslocal sqs get-queue-attributes \
  --queue-url "$EXECUCAO_DLQ_URL" \
  --attribute-names QueueArn \
  --query Attributes.QueueArn --output text)

EXECUCAO_QUEUE_INPUT=$(printf \
  '{"QueueName":"execucao-iniciar-diagnostico","Attributes":{"RedrivePolicy":"{\\"deadLetterTargetArn\\":\\"%s\\",\\"maxReceiveCount\\":\\"3\\"}"}}' \
  "$EXECUCAO_DLQ_ARN")
awslocal sqs create-queue --cli-input-json "$EXECUCAO_QUEUE_INPUT" >/dev/null

OS_DLQ_URL=$(awslocal sqs create-queue \
  --queue-name os-diagnostico-iniciado-dlq \
  --query QueueUrl --output text)
OS_DLQ_ARN=$(awslocal sqs get-queue-attributes \
  --queue-url "$OS_DLQ_URL" \
  --attribute-names QueueArn \
  --query Attributes.QueueArn --output text)
OS_QUEUE_INPUT=$(printf \
  '{"QueueName":"os-diagnostico-iniciado","Attributes":{"RedrivePolicy":"{\\"deadLetterTargetArn\\":\\"%s\\",\\"maxReceiveCount\\":\\"3\\"}"}}' \
  "$OS_DLQ_ARN")
OS_QUEUE_URL=$(awslocal sqs create-queue \
  --cli-input-json "$OS_QUEUE_INPUT" \
  --query QueueUrl --output text)
OS_QUEUE_ARN=$(awslocal sqs get-queue-attributes \
  --queue-url "$OS_QUEUE_URL" \
  --attribute-names QueueArn \
  --query Attributes.QueueArn --output text)

TOPIC_ARN=$(awslocal sns create-topic \
  --name oficina-eventos \
  --query TopicArn --output text)

OS_POLICY_INPUT=$(printf \
  '{"QueueUrl":"%s","Attributes":{"Policy":"{\\"Version\\":\\"2012-10-17\\",\\"Statement\\":[{\\"Effect\\":\\"Allow\\",\\"Principal\\":\\"*\\",\\"Action\\":\\"sqs:SendMessage\\",\\"Resource\\":\\"%s\\",\\"Condition\\":{\\"ArnEquals\\":{\\"aws:SourceArn\\":\\"%s\\"}}}]}"}}' \
  "$OS_QUEUE_URL" "$OS_QUEUE_ARN" "$TOPIC_ARN")
awslocal sqs set-queue-attributes --cli-input-json "$OS_POLICY_INPUT"

OS_SUBSCRIPTION_ARN=$(awslocal sns subscribe \
  --topic-arn "$TOPIC_ARN" \
  --protocol sqs \
  --notification-endpoint "$OS_QUEUE_ARN" \
  --attributes RawMessageDelivery=true \
  --query SubscriptionArn --output text)
awslocal sns set-subscription-attributes \
  --subscription-arn "$OS_SUBSCRIPTION_ARN" \
  --attribute-name FilterPolicy \
  --attribute-value '{"tipo":["DiagnosticoIniciadoMessage"]}'

OS_CONCLUIDO_DLQ_URL=$(awslocal sqs create-queue \
  --queue-name os-diagnostico-concluido-dlq \
  --query QueueUrl --output text)
OS_CONCLUIDO_DLQ_ARN=$(awslocal sqs get-queue-attributes \
  --queue-url "$OS_CONCLUIDO_DLQ_URL" \
  --attribute-names QueueArn \
  --query Attributes.QueueArn --output text)
OS_CONCLUIDO_QUEUE_INPUT=$(printf \
  '{"QueueName":"os-diagnostico-concluido","Attributes":{"RedrivePolicy":"{\\"deadLetterTargetArn\\":\\"%s\\",\\"maxReceiveCount\\":\\"3\\"}"}}' \
  "$OS_CONCLUIDO_DLQ_ARN")
OS_CONCLUIDO_QUEUE_URL=$(awslocal sqs create-queue \
  --cli-input-json "$OS_CONCLUIDO_QUEUE_INPUT" \
  --query QueueUrl --output text)
OS_CONCLUIDO_QUEUE_ARN=$(awslocal sqs get-queue-attributes \
  --queue-url "$OS_CONCLUIDO_QUEUE_URL" \
  --attribute-names QueueArn \
  --query Attributes.QueueArn --output text)

OS_CONCLUIDO_POLICY_INPUT=$(printf \
  '{"QueueUrl":"%s","Attributes":{"Policy":"{\\"Version\\":\\"2012-10-17\\",\\"Statement\\":[{\\"Effect\\":\\"Allow\\",\\"Principal\\":\\"*\\",\\"Action\\":\\"sqs:SendMessage\\",\\"Resource\\":\\"%s\\",\\"Condition\\":{\\"ArnEquals\\":{\\"aws:SourceArn\\":\\"%s\\"}}}]}"}}' \
  "$OS_CONCLUIDO_QUEUE_URL" "$OS_CONCLUIDO_QUEUE_ARN" "$TOPIC_ARN")
awslocal sqs set-queue-attributes --cli-input-json "$OS_CONCLUIDO_POLICY_INPUT"

OS_CONCLUIDO_SUBSCRIPTION_ARN=$(awslocal sns subscribe \
  --topic-arn "$TOPIC_ARN" \
  --protocol sqs \
  --notification-endpoint "$OS_CONCLUIDO_QUEUE_ARN" \
  --attributes RawMessageDelivery=true \
  --query SubscriptionArn --output text)
awslocal sns set-subscription-attributes \
  --subscription-arn "$OS_CONCLUIDO_SUBSCRIPTION_ARN" \
  --attribute-name FilterPolicy \
  --attribute-value '{"tipo":["DiagnosticoConcluidoMessage"]}'

echo "SNS/SQS locais configurados na regiao $REGION para a conta $ACCOUNT_ID."
