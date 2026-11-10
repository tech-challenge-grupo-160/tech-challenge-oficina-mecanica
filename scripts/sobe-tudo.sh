#!/usr/bin/env bash
#
# Sobe a infraestrutura inteira de um ambiente, do zero.
#
# Funciona em qualquer conta do AWS Academy Learner Lab: o nome do bucket de
# state carrega o id da conta, e todos os ARNs sao montados em tempo de
# execucao. Trocar a credencial e rodar de novo constroi tudo na conta nova.
#
# Uso:
#   ./scripts/sobe-tudo.sh                    # ambiente dev
#   ./scripts/sobe-tudo.sh --ambiente hom     # outro ambiente
#   ./scripts/sobe-tudo.sh --so-infra         # para antes das aplicacoes
#   ./scripts/sobe-tudo.sh --sim              # nao pergunta nada
#   ./scripts/sobe-tudo.sh --assumir-pipelines  # aponta as pipelines para a sua conta
#   ./scripts/sobe-tudo.sh --sem-datadog      # sobe sem Agent nem chave do Datadog
#
# CUSTO: o cluster EKS cobra US$ 0,10/hora enquanto existir e NAO e suspenso
# junto com a sessao do lab, diferente das instancias EC2. Com NAT, ALB e os dois RDS,
# ~US$ 5,40/dia por ambiente. Rode derruba-tudo.sh ao terminar.

if [ -z "${BASH_VERSION:-}" ]; then exec bash "$0" "$@"; fi
set -euo pipefail

DIR_SCRIPT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=comum.sh
source "$DIR_SCRIPT/comum.sh"

AMBIENTE="dev"
REGIAO="${AWS_REGION:-us-east-1}"
SO_INFRA=0
SEM_PERGUNTAR=0
ASSUMIR_PIPELINES=0
SEM_DATADOG=0

while [ $# -gt 0 ]; do
  case "$1" in
    --ambiente) AMBIENTE="$2"; shift 2 ;;
    --so-infra) SO_INFRA=1; shift ;;
    --sim)      SEM_PERGUNTAR=1; shift ;;
    --assumir-pipelines) ASSUMIR_PIPELINES=1; shift ;;
    --sem-datadog) SEM_DATADOG=1; shift ;;
    -h|--help)  sed -n '2,19p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) vermelho "Argumento desconhecido: $1"; exit 1 ;;
  esac
done

case "$AMBIENTE" in dev|hom|prod) ;; *)
  vermelho "Ambiente invalido: $AMBIENTE (use dev, hom ou prod)"; exit 1 ;;
esac

RAIZ="$(descobrir_raiz)"
K8S="$RAIZ/tech-challenge-infra-k8s"
BANCO="$RAIZ/tech-challenge-infra-database"
LAMBDA="$RAIZ/tech-challenge-lambda-auth"
APP="$RAIZ/tech-challenge-oficina-mecanica"
BILLING="$RAIZ/tech-challenge-oficina-mecanica-billing"
EXECUCAO="$RAIZ/tech-challenge-oficina-mecanica-execucao"

TOTAL=10
# Com --so-infra a etapa 2 e pulada, mas o fluxo vai ate a 6.
[ "$SO_INFRA" -eq 1 ] && TOTAL=6

# ============================================================ verificacoes

titulo "Verificacoes"

exigir_repos "$RAIZ"
exigir_ferramentas aws terraform
[ "$SO_INFRA" -eq 1 ] || exigir_ferramentas kubectl docker dotnet
exigir_credencial

CONTA="$(conta_atual)"
BUCKET="$(nome_do_bucket)"

verde "Credencial valida."
cinza "  conta:    $(echo "$CONTA" | sed 's/./*/g;s/\*\{4\}$//')$(echo "$CONTA" | tail -c 5)"
cinza "  ambiente: $AMBIENTE"
cinza "  regiao:   $REGIAO"
cinza "  state:    s3://$BUCKET"

exigir_lab_role || true

avisar_se_a_branch_diverge "$RAIZ" "$AMBIENTE" "CRIAR"

if [ "$SEM_PERGUNTAR" -eq 0 ]; then
  echo
  amarelo "Isto cria infraestrutura que COBRA:"
  echo "  - cluster EKS       ~US\$ 2,40/dia (nao para com a sessao do lab)"
  echo "  - NAT Gateway       ~US\$ 1,08/dia"
  echo "  - RDS multi-AZ (OS) ~US\$ 0,90/dia"
  echo "  - RDS da Execucao   ~US\$ 0,45/dia (Single-AZ em dev e hom)"
  echo "  - ALB               ~US\$ 0,54/dia"
  echo "  - SNS/SQS           cobranca por uso"
  echo
  confirmar "Continuar?" || { echo "Cancelado."; exit 0; }
fi

# ================================================================ bootstrap

titulo "Infraestrutura"
etapa 1 "$TOTAL" "Backend de state (S3 + DynamoDB)"

# Criado via AWS CLI, nao Terraform: ver garantir_backend_state em comum.sh
# para o motivo (o SCP do Learner Lab nega a leitura que o provider da AWS faz
# ao criar um aws_s3_bucket, inclusive em conta nova - nao so no refresh).
garantir_backend_state "$BUCKET" "$REGIAO"

# As pipelines precisam saber em que bucket esta o state, e a variavel vive nos
# repositorios da organizacao - compartilhada por todo mundo.
#
# Ai mora um risco que so aparece com mais de uma pessoa: cada membro tem a sua
# conta do Learner Lab, logo o seu bucket. Se dois rodarem este script, o
# segundo aponta as pipelines dos QUATRO repositorios para a conta dele, e os
# merges do primeiro passam a aplicar no ambiente errado - sem erro nenhum, e
# sem ninguem perceber.
#
# Por isso: sobrescreve em silencio so quando o valor ja e o mesmo, ou quando
# nao ha valor. Se outra conta ja for a dona, avisa e nao mexe.
if command -v gh >/dev/null 2>&1 && gh auth status >/dev/null 2>&1; then
  ATUAL="$(gh variable list --repo "tech-challenge-grupo-160/tech-challenge-infra-k8s" \
    --json name,value --jq '.[]|select(.name=="TF_STATE_BUCKET")|.value' 2>/dev/null || echo '')"

  if [ -n "$ATUAL" ] && [ "$ATUAL" != "$BUCKET" ] && [ "$ASSUMIR_PIPELINES" -eq 0 ]; then
    echo
    amarelo "  As pipelines apontam para OUTRA conta."
    cinza   "  atual:  ...$(echo "$ATUAL" | tail -c 5)"
    cinza   "  a sua:  ...$(echo "$BUCKET" | tail -c 5)"
    cinza   "  Nao mexi. Seu ambiente sobe normalmente, mas os merges do time"
    cinza   "  continuam aplicando na conta de quem configurou antes."
    cinza   "  Para assumir as pipelines: --assumir-pipelines"
  else
    for r in tech-challenge-oficina-mecanica tech-challenge-lambda-auth \
             tech-challenge-infra-k8s tech-challenge-infra-database \
             tech-challenge-oficina-mecanica-billing \
             tech-challenge-oficina-mecanica-execucao; do
      gh variable set TF_STATE_BUCKET --body "$BUCKET" \
        --repo "tech-challenge-grupo-160/$r" >/dev/null 2>&1 || true
    done
    cinza "  TF_STATE_BUCKET atualizado nos repositorios."
  fi
fi

# ------------------------------------------------------- funcoes Lambda
#
# ANTES da rede, e isto nao e preferencia. A `aws_lambda_permission` do gateway
# exige que a funcao exista: a API AddPermission devolve 404 se ela nao estiver
# publicada, e o apply inteiro falha.
#
# Num ambiente ja em uso o erro nunca aparece, porque as funcoes foram
# publicadas muito antes. Numa conta do zero, aparece sempre - descoberto em
# 31/08, na primeira subida completa por este script.
#
# Aqui vai so o codigo. Rede, variaveis de ambiente e VPC entram depois, quando
# subnets, security groups e segredos existirem.

CONTA_ROLE="arn:aws:iam::${CONTA}:role/LabRole"

if [ "$SO_INFRA" -eq 0 ]; then
  etapa 2 "$TOTAL" "Funcoes Lambda (codigo)"
  if ! dotnet lambda help >/dev/null 2>&1; then
    cinza "  Instalando Amazon.Lambda.Tools..."
    dotnet tool install -g Amazon.Lambda.Tools >/dev/null 2>&1 || true
  fi

dotnet lambda deploy-function "tc-grupo160-auth-${AMBIENTE}" \
    --project-location "$LAMBDA/Fiap.TechChallenge.OficinaMecanica.AuthLambda" \
    --configuration Release --function-role "$CONTA_ROLE" --region "$REGIAO"

# Mesmo artefato, outro handler. Sem --function-handler, publicaria a funcao
  # de autenticacao com o nome do authorizer, e sem erro nenhum no deploy.
  dotnet lambda deploy-function "tc-grupo160-authorizer-${AMBIENTE}" \
    --project-location "$LAMBDA/Fiap.TechChallenge.OficinaMecanica.AuthLambda" \
    --configuration Release --function-role "$CONTA_ROLE" --region "$REGIAO" \
    --function-handler "Fiap.TechChallenge.OficinaMecanica.AuthLambda::Fiap.TechChallenge.OficinaMecanica.AuthLambda.AuthorizerFunction::FunctionHandler"
fi

# ------------------------------------------------------------------- rede

etapa 3 "$TOTAL" "Rede, cluster, ECR, gateway e balanceador"
tf_init "$K8S/infra" "$AMBIENTE/rede.tfstate" "$BUCKET" "$REGIAO"

# Sem as funcoes publicadas, as permissoes do gateway nao podem ser criadas.
VAR_LAMBDAS="-var=lambdas_publicadas=true"
[ "$SO_INFRA" -eq 1 ] && VAR_LAMBDAS="-var=lambdas_publicadas=false"

# --sem-datadog desliga o Datadog so nesta subida, sem editar o inventory: o
# -var tem precedencia sobre o -var-file. Serve para quando nao ha chave valida
# a mao - em 10/10 uma chave recusada com 403 deixou o Agent em 2/3 e o Helm
# estourou o timeout. Num ambiente que ja tinha o Datadog, o apply remove o
# Agent e o segredo da chave; as Lambdas ficam sem instrumentacao, porque o
# segredo deixa de existir.
VAR_DATADOG=""
if [ "$SEM_DATADOG" -eq 1 ]; then
  VAR_DATADOG="-var=datadog_enabled=false"
  amarelo "  Subindo sem Datadog (--sem-datadog): sem Agent, APM nem metricas."
fi

# Chave do Datadog: do ambiente ou, na falta, do Secrets Manager.
#
# O comum.sh exporta uma chave dummy como padrao, para o Terraform validar sem
# segredo nenhum. Ate 14/09 este bloco so testava variavel vazia - que nunca
# estava vazia - e o Agent subia com a dummy: 403 no Datadog, readiness em 500
# e o helm_release estourando o timeout de 15 minutos sem dizer por que.
#
# Vai por variavel de ambiente, e nao por -var, para a chave nao aparecer na
# lista de processos.
DD_DUMMY="dummy_datadog_key_local"
if [ "$SEM_DATADOG" -eq 0 ] \
   && { [ -z "${TF_VAR_datadog_api_key:-}" ] || [ "$TF_VAR_datadog_api_key" = "$DD_DUMMY" ]; }; then
  SECRET_DD_KEY="$(aws secretsmanager get-secret-value --secret-id "tc-grupo160/${AMBIENTE}/datadog-api-key" --region "$REGIAO" --query SecretString --output text 2>/dev/null || echo '')"
  if [ -n "$SECRET_DD_KEY" ] && [ "$SECRET_DD_KEY" != "$DD_DUMMY" ]; then
    export TF_VAR_datadog_api_key="$SECRET_DD_KEY"
  fi
fi

# Numa conta nova o segredo ainda nao existe - quem cria e este mesmo apply, a
# partir da variavel. Melhor parar aqui do que esperar o Helm desistir.
#
# A mensagem nao oferece "grave o segredo no Secrets Manager" como saida, e ate
# 15/09 oferecia: numa conta nova o segredo criado a mao colide com o
# aws_secretsmanager_secret do Terraform, e o apply falha com
# ResourceExistsException. A unica saida segura aqui e a variavel.
if [ "$SEM_DATADOG" -eq 0 ] \
   && grep -qE '^[[:space:]]*datadog_enabled[[:space:]]*=[[:space:]]*true' "$K8S/infra/inventories/$AMBIENTE/terraform.tfvars" \
   && [ "$TF_VAR_datadog_api_key" = "$DD_DUMMY" ]; then
  vermelho "  Datadog ligado em inventories/$AMBIENTE, mas sem chave real."
  echo "  Carregue a chave no terminal e rode de novo:"
  echo "    read -rsp 'Chave do Datadog: ' TF_VAR_datadog_api_key && echo && export TF_VAR_datadog_api_key"
  echo "  Ou suba sem Datadog: bash scripts/sobe-tudo.sh --ambiente $AMBIENTE --sem-datadog"
  echo "  Nao crie o segredo tc-grupo160/$AMBIENTE/datadog-api-key a mao: quem cria e o"
  echo "  apply, e um segredo ja existente faz o Terraform falhar."
  echo "  Passo a passo: docs/CICLO-DE-VIDA.md, secao Chave do Datadog."
  exit 1
fi

# Credenciais do Mercado Pago (F4-06/F4-39): do ambiente ou, na falta, do
# segredo que um apply anterior (da pipeline ou daqui) ja gravou nesta conta.
#
# O infra-k8s grava o segredo tc-grupo160/<amb>/mercadopago a partir de
# TF_VAR_mercadopago_access_token e TF_VAR_mercadopago_webhook_secret. Um apply
# sem elas cria o segredo vazio, ou APAGA o valor que ja estava la, e o Billing
# nao sobe. Por isso o script reaproveita o valor existente antes de aplicar.
#
# Sem valor em lugar nenhum o script so avisa, sem parar: o pagamento simulado
# nao precisa do Mercado Pago, e o resto do ambiente sobe normalmente.
# Variavel de ambiente, e nao -var, pelo mesmo motivo da chave do Datadog.
if [ -z "${TF_VAR_mercadopago_access_token:-}" ]; then
  MP_SEGREDO="$(aws secretsmanager get-secret-value --secret-id "tc-grupo160/${AMBIENTE}/mercadopago" \
    --region "$REGIAO" --query SecretString --output text 2>/dev/null || echo '')"
  mp_valor() { printf '%s\n' "$MP_SEGREDO" | sed -n "s/.*\"$1\" *: *\"\\([^\"]*\\)\".*/\\1/p"; }
  MP_TOKEN="$(mp_valor access_token)"
  MP_WEBHOOK="$(mp_valor webhook_secret)"
  if [ -n "$MP_TOKEN" ] && [ -n "$MP_WEBHOOK" ]; then
    export TF_VAR_mercadopago_access_token="$MP_TOKEN"
    export TF_VAR_mercadopago_webhook_secret="$MP_WEBHOOK"
    cinza "  Credenciais do Mercado Pago reaproveitadas do Secrets Manager."
  fi
  unset MP_SEGREDO MP_TOKEN MP_WEBHOOK
fi

if [ -z "${TF_VAR_mercadopago_access_token:-}" ]; then
  amarelo "  Sem credenciais do Mercado Pago: o segredo tc-grupo160/$AMBIENTE/mercadopago fica vazio."
  cinza   "  O pagamento simulado funciona. Para o Checkout Pro, carregue e rode de novo:"
  cinza   "    read -rsp 'Access token: ' TF_VAR_mercadopago_access_token && echo && export TF_VAR_mercadopago_access_token"
  cinza   "    read -rsp 'Webhook secret: ' TF_VAR_mercadopago_webhook_secret && echo && export TF_VAR_mercadopago_webhook_secret"
  cinza   "  As credenciais de teste estao com quem cuida da F4-06 (issue #137)."
fi

# shellcheck disable=SC2086
terraform -chdir="$K8S/infra" apply -auto-approve -input=false \
  -var-file="inventories/$AMBIENTE/terraform.tfvars" $VAR_LAMBDAS $VAR_DATADOG
verde "  Ambiente $AMBIENTE aplicado."

# ------------------------------------------------------------------ banco
#
# Depois da rede, sempre: o Terraform do banco le vpc_id, subnets e o security
# group do state da rede. Invertendo a ordem, ele falha procurando um state que
# ainda nao existe.

etapa 4 "$TOTAL" "Banco de dados gerenciado"
tf_init "$BANCO" "$AMBIENTE/banco.tfstate" "$BUCKET" "$REGIAO"
terraform -chdir="$BANCO" apply -auto-approve -input=false \
  -var-file="inventories/$AMBIENTE/terraform.tfvars"
verde "  RDS aplicado."

# -------------------------------------------------------------- mensageria
#
# Cada servico e dono do seu topico e das filas que consome. Os tres topicos
# precisam existir antes de qualquer assinatura; por isso o bootstrap aplica
# primeiro so os topicos, depois a configuracao completa de cada servico.

etapa 5 "$TOTAL" "Topicos, filas e DLQs SNS/SQS por servico"
MENSAGERIA_DIRS=("$APP/infra" "$BILLING/infra" "$EXECUCAO/infra")
MENSAGERIA_STATES=("messaging-os.tfstate" "messaging-billing.tfstate" "messaging-execucao.tfstate")

for i in "${!MENSAGERIA_DIRS[@]}"; do
  tf_init "${MENSAGERIA_DIRS[$i]}" "$AMBIENTE/${MENSAGERIA_STATES[$i]}" "$BUCKET" "$REGIAO"
  terraform -chdir="${MENSAGERIA_DIRS[$i]}" apply -auto-approve -input=false \
    -target=aws_sns_topic.events \
    -var="environment=$AMBIENTE" -var="region=$REGIAO"
done

for i in "${!MENSAGERIA_DIRS[@]}"; do
  terraform -chdir="${MENSAGERIA_DIRS[$i]}" apply -auto-approve -input=false \
    -var="environment=$AMBIENTE" -var="region=$REGIAO"
done
verde "  Mensageria aplicada nos tres servicos."

GATEWAY="$(terraform -chdir="$K8S/infra" output -raw gateway_url 2>/dev/null || echo '')"

etapa 6 "$TOTAL" "Conferindo o que subiu"
CLUSTER="$(terraform -chdir="$K8S/infra" output -raw cluster_nome 2>/dev/null || echo '')"
if [ -z "$CLUSTER" ] || [ "$CLUSTER" = "null" ]; then
  amarelo "  criar_cluster esta desligado em inventories/$AMBIENTE."
  amarelo "  Sem cluster nao ha onde publicar a aplicacao."
  SO_INFRA=1
else
  verde "  Cluster: $CLUSTER"
fi

if [ "$SO_INFRA" -eq 1 ]; then
  titulo "Pronto"
  echo "Gateway: $GATEWAY"
  echo
  echo "Para publicar as aplicacoes, rode de novo sem --so-infra."
  exit 0
fi

# ============================================================== aplicacoes

titulo "Aplicacoes"

SUFIXO="$AMBIENTE"
ECR="$(terraform -chdir="$K8S/infra" output -raw ecr_api_url)"

# ---------------------------------------------------- configuracao das lambdas
#
# O codigo ja foi publicado na etapa 2 - tinha que ser, senao a permissao do
# gateway falharia. Aqui entram rede e variaveis, que dependem de subnets,
# security groups e segredos criados nas etapas seguintes.
#
# As funcoes nao sao criadas pelo Terraform: gerenciar aqui e la faria os dois
# disputarem o mesmo recurso. Por isso tambem nao somem no `terraform destroy` -
# ver derruba-tudo.sh.

etapa 7 "$TOTAL" "Rede e variaveis das funcoes"

SEGREDO_JWT="tc-grupo160/${SUFIXO}/jwt-signing-key"
SEGREDO_BANCO="tc-grupo160/${SUFIXO}/banco"

# A funcao de autenticacao precisa da VPC para alcancar o RDS; o authorizer nao
# toca no banco e fica fora, evitando a ENI e o cold start que ela custa.
SUBNETS="$(aws ec2 describe-subnets \
  --filters "Name=tag:Name,Values=tc-grupo160-${SUFIXO}-privada-*" \
  --query 'Subnets[].SubnetId' --output json | tr -d ' \n')"
SG_LAMBDA="$(aws ec2 describe-security-groups \
  --filters "Name=tag:Name,Values=tc-grupo160-${SUFIXO}-lambda" \
  --query 'SecurityGroups[0].GroupId' --output text)"

aws lambda wait function-updated --function-name "tc-grupo160-auth-${SUFIXO}"
aws lambda update-function-configuration \
  --function-name "tc-grupo160-auth-${SUFIXO}" \
  --environment "{\"Variables\":{\"JWT_SECRET_ID\":\"$SEGREDO_JWT\",\"DB_SECRET_ID\":\"$SEGREDO_BANCO\",\"DD_TRACE_ENABLED\":\"true\"}}" \
  --vpc-config "{\"SubnetIds\":${SUBNETS},\"SecurityGroupIds\":[\"${SG_LAMBDA}\"]}" >/dev/null
aws lambda wait function-updated --function-name "tc-grupo160-auth-${SUFIXO}"

aws lambda wait function-updated --function-name "tc-grupo160-authorizer-${SUFIXO}"
aws lambda update-function-configuration \
  --function-name "tc-grupo160-authorizer-${SUFIXO}" \
  --environment "{\"Variables\":{\"JWT_SECRET_ID\":\"$SEGREDO_JWT\",\"JWT_ISSUER\":\"Fiap.TechChallenge.OficinaMecanica\",\"JWT_AUDIENCE\":\"Fiap.TechChallenge.OficinaMecanica\",\"DD_TRACE_ENABLED\":\"true\"}}" >/dev/null
verde "  Funcoes configuradas."

# A instrumentacao vem depois da configuracao das variaveis da aplicacao:
# update-function-configuration substitui o mapa inteiro e apagaria as DD_* se
# o datadog-ci rodasse antes. O datadog-ci adiciona as camadas e as variaveis
# necessarias sem colocar a API key em texto puro na funcao.
DATADOG_SECRET_ARN="$(terraform -chdir="$K8S/infra" output -raw datadog_api_key_secret_arn 2>/dev/null || echo '')"
if [ -n "$DATADOG_SECRET_ARN" ] && [ "$DATADOG_SECRET_ARN" != "null" ]; then
  exigir_ferramentas npx
  DATADOG_API_KEY_SECRET_ARN="$DATADOG_SECRET_ARN" \
    DATADOG_SITE="datadoghq.com" \
    npx --yes @datadog/datadog-ci@latest lambda instrument \
    -f "tc-grupo160-auth-${AMBIENTE}" \
    -f "tc-grupo160-authorizer-${AMBIENTE}" \
    -r "$REGIAO" -v 25 -e 99
  verde "  Lambdas instrumentadas com Datadog."
fi
# -------------------------------------------------------------------- API

etapa 8 "$TOTAL" "API no cluster"
aws eks update-kubeconfig --region "$REGIAO" --name "$CLUSTER" >/dev/null

SHA="$(git -C "$APP" rev-parse --short=12 HEAD)"
aws ecr get-login-password --region "$REGIAO" \
  | docker login --username AWS --password-stdin "${ECR%%/*}" >/dev/null 2>&1

docker build -q -f "$APP/docker/backend/Dockerfile" \
  -t "${ECR}:${SHA}" -t "${ECR}:latest" "$APP" >/dev/null
docker push -q "${ECR}:${SHA}" >/dev/null
docker push -q "${ECR}:latest" >/dev/null

kubectl apply -f "$K8S/k8s/nuvem/namespace.yaml" >/dev/null

# O Secret e montado do Secrets Manager a cada deploy, nunca versionado. Sem jq
# de proposito: ele nao vem no Git Bash do Windows.
CONN="$(aws secretsmanager get-secret-value --secret-id "$SEGREDO_BANCO" --region "$REGIAO" \
  --query SecretString --output text \
  | grep -o '"connectionString":"[^"]*"' | sed 's/^"connectionString":"//; s/"$//')"
JWT="$(aws secretsmanager get-secret-value --secret-id "$SEGREDO_JWT" --region "$REGIAO" \
  --query SecretString --output text)"
SEGREDO_MENSAGERIA="tc-grupo160/${SUFIXO}/os/messaging"
MENSAGERIA="$(aws secretsmanager get-secret-value --secret-id "$SEGREDO_MENSAGERIA" --region "$REGIAO" \
  --query SecretString --output text)"
valor_json() {
  local chave="$1"
  printf '%s\n' "$MENSAGERIA" \
    | sed -n "s/.*\"$chave\":\"\\([^\"]*\\)\".*/\\1/p"
}

MSG_TOPIC_ARN="$(valor_json Messaging__EventTopicArn)"
MSG_BILLING_EVENTS_URL="$(valor_json Messaging__BillingEventsQueueUrl)"
MSG_EXECUTION_EVENTS_URL="$(valor_json Messaging__ExecutionEventsQueueUrl)"
MSG_BILLING_COMMANDS_URL="$(valor_json Messaging__BillingCommandsQueueUrl)"
MSG_EXECUTION_COMMANDS_URL="$(valor_json Messaging__ExecutionCommandsQueueUrl)"
if [ -z "$MSG_TOPIC_ARN" ] || [ -z "$MSG_BILLING_EVENTS_URL" ] \
   || [ -z "$MSG_EXECUTION_EVENTS_URL" ] || [ -z "$MSG_BILLING_COMMANDS_URL" ] \
   || [ -z "$MSG_EXECUTION_COMMANDS_URL" ]; then
  vermelho "ERRO: configuracao de mensageria ausente ou incompleta em $SEGREDO_MENSAGERIA."
  exit 1
fi

kubectl create secret generic api-secret --namespace oficina-mecanica \
  --from-literal=ConnectionStrings__DefaultConnection="$CONN" \
  --from-literal=Jwt__SecretKey="$JWT" \
  --from-literal=Messaging__EventTopicArn="$MSG_TOPIC_ARN" \
  --from-literal=Messaging__BillingEventsQueueUrl="$MSG_BILLING_EVENTS_URL" \
  --from-literal=Messaging__ExecutionEventsQueueUrl="$MSG_EXECUTION_EVENTS_URL" \
  --from-literal=Messaging__BillingCommandsQueueUrl="$MSG_BILLING_COMMANDS_URL" \
  --from-literal=Messaging__ExecutionCommandsQueueUrl="$MSG_EXECUTION_COMMANDS_URL" \
  --dry-run=client -o yaml | kubectl apply -f - >/dev/null

TMP="$(mktemp -d)"
cp -r "$K8S/k8s/." "$TMP/"
# SHA pode ser formado apenas por algarismos. Sem aspas, o YAML o interpreta
# como numero e o Kustomize rejeita `images[].newTag`, que precisa ser string.
sed -i "s|newTag: .*|newTag: \"${SHA}\"|; s|newName: .*|newName: ${ECR}|" "$TMP/nuvem/kustomization.yaml"
# DD_ENV do patch-api.yaml (unified service tagging) vem com o mesmo
# placeholder resolvido aqui pelo motivo do newTag acima.
sed -i "s|__AMBIENTE__|${AMBIENTE}|g" "$TMP/nuvem/patch-api.yaml"
kubectl apply -k "$TMP/nuvem" >/dev/null

# O Cluster Autoscaler vive em kube-system, fora do overlay da aplicacao. E ele
# que cria node quando o HPA pede mais pod do que cabe. Os dois placeholders sao
# resolvidos aqui pelo mesmo motivo do newTag acima: o manifest serve os tres
# ambientes, e o nome do cluster entra na tag de descoberta.
sed -i "s|__CLUSTER__|${CLUSTER}|g; s|__REGIAO__|${REGIAO}|g" \
  "$TMP/cluster-autoscaler/deployment.yaml"
kubectl apply -k "$TMP/cluster-autoscaler" >/dev/null

rm -rf "$TMP"

cinza "  Aguardando rollout (a migration roda no startup)..."
kubectl rollout status deployment/oficina-mecanica-api \
  --namespace oficina-mecanica --timeout=420s
verde "  API no ar."

# ------------------------------------------------------ Billing e Execucao
#
# O mesmo deploy que a pipeline reutilizavel faz (tech-challenge-pipelines,
# F4-08), a partir das pastas locais: imagem no ECR com o SHA do repositorio,
# Secret montado do Secrets Manager e os manifests k8s/ de cada servico.
#
# As chaves do Secret sao as do segredo de mensageria da F4-11 e as mesmas do
# input `segredos` da pipeline de cada servico (F4-38). A Execucao recebe
# tambem a connection string do RDS proprio (F4-12), aplicado na etapa 4 pelo
# infra-database. O DynamoDB do Billing entra quando a F4-13 existir.
#
# Rollout que nao fica pronto so gera aviso, sem parar o script: um servico
# fora do ar nao pode impedir o OS Service e o teste de fumaca.

etapa 9 "$TOTAL" "Billing e Execucao no cluster"
REGISTRY="${ECR%%/*}"

deploy_servico() {
  local servico="$1" pasta="$2"
  local nome="oficina-mecanica-${servico}"
  local imagem="${REGISTRY}/tc-grupo160-${AMBIENTE}-${servico}"
  local sha segredo json chave valor tmp
  local args=()

  sha="$(git -C "$pasta" rev-parse --short=12 HEAD)"
  docker build -q -f "$pasta/docker/backend/Dockerfile" \
    -t "${imagem}:${sha}" -t "${imagem}:latest" "$pasta" >/dev/null
  docker push -q "${imagem}:${sha}" >/dev/null
  docker push -q "${imagem}:latest" >/dev/null

  segredo="tc-grupo160/${SUFIXO}/${servico}/messaging"
  json="$(aws secretsmanager get-secret-value --secret-id "$segredo" --region "$REGIAO" \
    --query SecretString --output text)"
  for chave in Messaging__EventTopicArn Messaging__CommandsQueueUrl Messaging__OsEventsQueueUrl; do
    valor="$(printf '%s\n' "$json" | sed -n "s/.*\"$chave\":\"\\([^\"]*\\)\".*/\\1/p")"
    if [ -z "$valor" ]; then
      vermelho "ERRO: $chave ausente em $segredo."
      exit 1
    fi
    args+=(--from-literal="${chave}=${valor}")
  done

  # So a Execucao tem banco relacional proprio, e so ela recebe esta credencial.
  if [ "$servico" = "execucao" ]; then
    segredo="tc-grupo160/${SUFIXO}/execucao/banco"
    json="$(aws secretsmanager get-secret-value --secret-id "$segredo" --region "$REGIAO" \
      --query SecretString --output text)"
    valor="$(printf '%s\n' "$json" | sed -n 's/.*"connectionString":"\([^"]*\)".*/\1/p')"
    if [ -z "$valor" ]; then
      vermelho "ERRO: connectionString ausente em $segredo."
      exit 1
    fi
    args+=(--from-literal="ConnectionStrings__DefaultConnection=${valor}")
  fi

  kubectl create secret generic "${nome}-secret" --namespace oficina-mecanica \
    "${args[@]}" --dry-run=client -o yaml | kubectl apply -f - >/dev/null

  tmp="$(mktemp -d)"
  cp -r "$pasta/k8s/." "$tmp/"
  # Aspas no newTag pelo mesmo motivo do OS Service: SHA so de algarismos.
  sed -i "s|newTag: .*|newTag: \"${sha}\"|; s|newName: .*|newName: ${imagem}|" "$tmp/kustomization.yaml"
  grep -rl "__AMBIENTE__" "$tmp" | xargs -r sed -i "s|__AMBIENTE__|${AMBIENTE}|g"
  kubectl apply -k "$tmp" >/dev/null
  rm -rf "$tmp"

  if kubectl rollout status "deployment/$nome" --namespace oficina-mecanica --timeout=300s >/dev/null 2>&1; then
    verde "  $nome no ar."
  else
    amarelo "  $nome nao ficou pronto em 5 min. Diagnostico:"
    cinza   "    kubectl describe pods -n oficina-mecanica -l app=$nome | tail -30"
    cinza   "    kubectl logs -n oficina-mecanica -l app=$nome --tail=80"
  fi
}

deploy_servico billing  "$BILLING"
deploy_servico execucao "$EXECUCAO"

# ============================================================= verificacao

etapa 10 "$TOTAL" "Teste de fumaca"
sleep 10
CODIGO="$(curl -s -o /dev/null -w '%{http_code}' -X POST "$GATEWAY/auth" \
  -H 'Content-Type: application/json' -d '{"documento":"476.548.668-01"}' || echo 000)"
if [ "$CODIGO" = "200" ]; then
  verde "  POST /auth respondeu 200."
else
  amarelo "  POST /auth respondeu $CODIGO."
  amarelo "  O VPC Link leva ~3 min para propagar depois de criado. Tente de novo."
fi

titulo "Pronto"
echo "Gateway:  $GATEWAY"
echo "Cluster:  $CLUSTER"
echo
amarelo "Lembre de rodar ./scripts/derruba-tudo.sh ao terminar."
