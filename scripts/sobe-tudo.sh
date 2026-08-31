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

while [ $# -gt 0 ]; do
  case "$1" in
    --ambiente) AMBIENTE="$2"; shift 2 ;;
    --so-infra) SO_INFRA=1; shift ;;
    --sim)      SEM_PERGUNTAR=1; shift ;;
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

TOTAL=7
[ "$SO_INFRA" -eq 1 ] && TOTAL=4

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
if [ -f "$STATE_BOOT" ] && ! grep -q "$BUCKET" "$STATE_BOOT" 2>/dev/null; then
  amarelo "  State local do bootstrap e de outra conta. Movendo para .antigo."
  mv "$STATE_BOOT" "$STATE_BOOT.antigo-$(date +%Y%m%d%H%M%S)"
fi

terraform -chdir="$K8S/bootstrap" init -input=false >/dev/null
terraform -chdir="$K8S/bootstrap" apply -auto-approve -input=false >/dev/null
verde "  Backend pronto: s3://$BUCKET"

# Sem isto as pipelines apontariam para o bucket da conta antiga.
if command -v gh >/dev/null 2>&1 && gh auth status >/dev/null 2>&1; then
  for r in tech-challenge-oficina-mecanica tech-challenge-lambda-auth \
           tech-challenge-infra-k8s tech-challenge-infra-database; do
    gh variable set TF_STATE_BUCKET --body "$BUCKET" \
      --repo "tech-challenge-grupo-160/$r" >/dev/null 2>&1 || true
  done
  cinza "  TF_STATE_BUCKET atualizado nos repositorios."
fi

# ------------------------------------------------------------------- rede

etapa 2 "$TOTAL" "Rede, cluster, ECR, gateway e balanceador"
tf_init "$K8S/infra" "$AMBIENTE/rede.tfstate" "$BUCKET" "$REGIAO"
terraform -chdir="$K8S/infra" apply -auto-approve -input=false \
  -var-file="inventories/$AMBIENTE/terraform.tfvars"
verde "  Ambiente $AMBIENTE aplicado."

# ------------------------------------------------------------------ banco
#
# Depois da rede, sempre: o Terraform do banco le vpc_id, subnets e o security
# group do state da rede. Invertendo a ordem, ele falha procurando um state que
# ainda nao existe.

etapa 3 "$TOTAL" "Banco de dados gerenciado"
tf_init "$BANCO" "$AMBIENTE/banco.tfstate" "$BUCKET" "$REGIAO"
terraform -chdir="$BANCO" apply -auto-approve -input=false \
  -var-file="inventories/$AMBIENTE/terraform.tfvars"
verde "  RDS aplicado."

GATEWAY="$(terraform -chdir="$K8S/infra" output -raw gateway_url 2>/dev/null || echo '')"

etapa 4 "$TOTAL" "Conferindo o que subiu"
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

# ---------------------------------------------------------------- lambdas
#
# As duas funcoes saem do MESMO artefato, com handlers diferentes. Nao sao
# criadas pelo Terraform: gerenciar aqui e la faria os dois disputarem o mesmo
# recurso. Por isso tambem nao somem no `terraform destroy` - ver
# derruba-tudo.sh.

etapa 5 "$TOTAL" "Funcoes Lambda"
if ! dotnet lambda help >/dev/null 2>&1; then
  cinza "  Instalando Amazon.Lambda.Tools..."
  dotnet tool install -g Amazon.Lambda.Tools >/dev/null 2>&1 || true
fi

ROLE="arn:aws:iam::${CONTA}:role/LabRole"
SEGREDO_JWT="tc-grupo160/${SUFIXO}/jwt-signing-key"
SEGREDO_BANCO="tc-grupo160/${SUFIXO}/banco"

dotnet lambda deploy-function "tc-grupo160-auth-${SUFIXO}" \
  --project-location "$LAMBDA/Fiap.TechChallenge.OficinaMecanica.AuthLambda" \
  --configuration Release --function-role "$ROLE" --region "$REGIAO" >/dev/null

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
  --environment "{\"Variables\":{\"JWT_SECRET_ID\":\"$SEGREDO_JWT\",\"DB_SECRET_ID\":\"$SEGREDO_BANCO\"}}" \
  --vpc-config "{\"SubnetIds\":${SUBNETS},\"SecurityGroupIds\":[\"${SG_LAMBDA}\"]}" >/dev/null
aws lambda wait function-updated --function-name "tc-grupo160-auth-${SUFIXO}"

dotnet lambda deploy-function "tc-grupo160-authorizer-${SUFIXO}" \
  --project-location "$LAMBDA/Fiap.TechChallenge.OficinaMecanica.AuthLambda" \
  --configuration Release --function-role "$ROLE" --region "$REGIAO" \
  --function-handler "Fiap.TechChallenge.OficinaMecanica.AuthLambda::Fiap.TechChallenge.OficinaMecanica.AuthLambda.AuthorizerFunction::FunctionHandler" >/dev/null

aws lambda wait function-updated --function-name "tc-grupo160-authorizer-${SUFIXO}"
aws lambda update-function-configuration \
  --function-name "tc-grupo160-authorizer-${SUFIXO}" \
  --environment "{\"Variables\":{\"JWT_SECRET_ID\":\"$SEGREDO_JWT\",\"JWT_ISSUER\":\"Fiap.TechChallenge.OficinaMecanica\",\"JWT_AUDIENCE\":\"Fiap.TechChallenge.OficinaMecanica\"}}" >/dev/null
verde "  Autenticacao e authorizer publicados."

# O authorizer so pode ser referenciado depois de existir. Reaplicar a rede
# agora cria o aws_apigatewayv2_authorizer, que na primeira passada e ignorado
# se a funcao ainda nao existia.
terraform -chdir="$K8S/infra" apply -auto-approve -input=false \
  -var-file="inventories/$AMBIENTE/terraform.tfvars" >/dev/null

# -------------------------------------------------------------------- API

etapa 6 "$TOTAL" "API no cluster"
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
CONN="$(aws secretsmanager get-secret-value --secret-id "$SEGREDO_BANCO" \
  --query SecretString --output text \
  | grep -o '"connectionString":"[^"]*"' | sed 's/^"connectionString":"//; s/"$//')"
JWT="$(aws secretsmanager get-secret-value --secret-id "$SEGREDO_JWT" \
  --query SecretString --output text)"

kubectl create secret generic api-secret --namespace oficina-mecanica \
  --from-literal=ConnectionStrings__DefaultConnection="$CONN" \
  --from-literal=Jwt__SecretKey="$JWT" \
  --dry-run=client -o yaml | kubectl apply -f - >/dev/null

TMP="$(mktemp -d)"
cp -r "$K8S/k8s/." "$TMP/"
sed -i "s|newTag: .*|newTag: ${SHA}|; s|newName: .*|newName: ${ECR}|" "$TMP/nuvem/kustomization.yaml"
kubectl apply -k "$TMP/nuvem" >/dev/null
rm -rf "$TMP"

cinza "  Aguardando rollout (a migration roda no startup)..."
kubectl rollout status deployment/oficina-mecanica-api \
  --namespace oficina-mecanica --timeout=420s
verde "  API no ar."

# ============================================================= verificacao

etapa 7 "$TOTAL" "Teste de fumaca"
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
