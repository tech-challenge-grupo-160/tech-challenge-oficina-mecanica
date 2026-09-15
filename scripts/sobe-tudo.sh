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
#
# CUSTO: o cluster EKS cobra US$ 0,10/hora enquanto existir e NAO e suspenso
# junto com a sessao do lab, diferente das instancias EC2. Com o NAT Gateway,
# ~US$ 3,50/dia por ambiente. Rode derruba-tudo.sh ao terminar.

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

while [ $# -gt 0 ]; do
  case "$1" in
    --ambiente) AMBIENTE="$2"; shift 2 ;;
    --so-infra) SO_INFRA=1; shift ;;
    --sim)      SEM_PERGUNTAR=1; shift ;;
    --assumir-pipelines) ASSUMIR_PIPELINES=1; shift ;;
    -h|--help)  sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
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

TOTAL=8
# Com --so-infra a etapa 2 e pulada, mas o fluxo vai ate a 5.
[ "$SO_INFRA" -eq 1 ] && TOTAL=5

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
  echo "  - RDS multi-AZ      ~US\$ 0,90/dia"
  echo "  - ALB               ~US\$ 0,54/dia"
  echo
  confirmar "Continuar?" || { echo "Cancelado."; exit 0; }
fi

# ================================================================ bootstrap

titulo "Infraestrutura"
etapa 1 "$TOTAL" "Backend de state (S3 + DynamoDB)"

# O state do bootstrap e local e fica fora do git. Numa conta nova ele nao
# existe, e o apply cria o bucket do zero. Se existir apontando para OUTRA
# conta, reaproveita-lo faria o Terraform tentar reconciliar recursos que nao
# existem aqui - por isso a checagem.
STATE_BOOT="$K8S/bootstrap/terraform.tfstate"
if [ -f "$STATE_BOOT" ]; then
  # Se o state local existir mas nao contiver o bucket nem recursos registrados
  # para a conta atual, move para antigo para forcar o Terraform a importar ou usar o correto.
  if ! grep -q "$BUCKET" "$STATE_BOOT" 2>/dev/null || ! grep -q "aws_s3_bucket" "$STATE_BOOT" 2>/dev/null; then
    amarelo "  State local do bootstrap invalido ou de outra conta. Movendo para .antigo."
    mv "$STATE_BOOT" "$STATE_BOOT.antigo-$(date +%Y%m%d%H%M%S)"
  fi
fi

terraform -chdir="$K8S/bootstrap" init -input=false >/dev/null

# Com o bucket ja de pe, o apply roda SEM refresh. Nao e teimosia: em 04/09 o
# SCP do Learner Lab passou a negar `s3:GetBucketObjectLockConfiguration`, e o
# provider da AWS faz essa leitura toda vez que atualiza um aws_s3_bucket. O
# apply morria com AccessDenied antes de chegar em qualquer outra etapa.
#
#   AccessDenied: ... explicit deny in a service control policy
#
# Nao ha o que perder aqui. Bucket e tabela de lock sao criados uma vez e nunca
# mudam, e o `plan -refresh=false` confirma que o state ja bate com a
# configuracao. O caminho de conta nova continua igual, com refresh, porque la
# ha recurso a criar de verdade - se o SCP tambem barrar aquele fluxo, o erro
# aparece na cara e nao escondido atras de um -refresh=false.
if aws s3api head-bucket --bucket "$BUCKET" >/dev/null 2>&1; then
  terraform -chdir="$K8S/bootstrap" apply -auto-approve -input=false -refresh=false >/dev/null
else
  terraform -chdir="$K8S/bootstrap" apply -auto-approve -input=false >/dev/null
fi
verde "  Backend pronto: s3://$BUCKET"

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
             tech-challenge-infra-k8s tech-challenge-infra-database; do
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
if [ -z "${TF_VAR_datadog_api_key:-}" ] || [ "$TF_VAR_datadog_api_key" = "$DD_DUMMY" ]; then
  SECRET_DD_KEY="$(aws secretsmanager get-secret-value --secret-id "tc-grupo160/${AMBIENTE}/datadog-api-key" --region "$REGIAO" --query SecretString --output text 2>/dev/null || echo '')"
  if [ -n "$SECRET_DD_KEY" ] && [ "$SECRET_DD_KEY" != "$DD_DUMMY" ]; then
    export TF_VAR_datadog_api_key="$SECRET_DD_KEY"
  fi
fi

# Numa conta nova o segredo ainda nao existe - quem cria e este mesmo apply, a
# partir da variavel. Melhor parar aqui do que esperar o Helm desistir.
if grep -qE '^[[:space:]]*datadog_enabled[[:space:]]*=[[:space:]]*true' "$K8S/infra/inventories/$AMBIENTE/terraform.tfvars" \
   && [ "$TF_VAR_datadog_api_key" = "$DD_DUMMY" ]; then
  vermelho "  Datadog ligado em inventories/$AMBIENTE, mas sem chave real."
  echo "  Exporte TF_VAR_datadog_api_key ou grave tc-grupo160/$AMBIENTE/datadog-api-key"
  echo "  no Secrets Manager e rode de novo."
  exit 1
fi

# shellcheck disable=SC2086
terraform -chdir="$K8S/infra" apply -auto-approve -input=false \
  -var-file="inventories/$AMBIENTE/terraform.tfvars" $VAR_LAMBDAS
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

GATEWAY="$(terraform -chdir="$K8S/infra" output -raw gateway_url 2>/dev/null || echo '')"

etapa 5 "$TOTAL" "Conferindo o que subiu"
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

etapa 6 "$TOTAL" "Rede e variaveis das funcoes"

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

etapa 7 "$TOTAL" "API no cluster"
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

kubectl create secret generic api-secret --namespace oficina-mecanica \
  --from-literal=ConnectionStrings__DefaultConnection="$CONN" \
  --from-literal=Jwt__SecretKey="$JWT" \
  --dry-run=client -o yaml | kubectl apply -f - >/dev/null

TMP="$(mktemp -d)"
cp -r "$K8S/k8s/." "$TMP/"
sed -i "s|newTag: .*|newTag: ${SHA}|; s|newName: .*|newName: ${ECR}|" "$TMP/nuvem/kustomization.yaml"
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

# ============================================================= verificacao

etapa 8 "$TOTAL" "Teste de fumaca"
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
