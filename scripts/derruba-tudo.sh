#!/usr/bin/env bash
#
# Destroi a infraestrutura de um ambiente e confere que nada ficou cobrando.
#
# Uso:
#   ./scripts/derruba-tudo.sh                   # ambiente dev
#   ./scripts/derruba-tudo.sh --ambiente hom
#   ./scripts/derruba-tudo.sh --com-bootstrap   # remove tambem o bucket de state
#   ./scripts/derruba-tudo.sh --so-conferir     # nao destroi, so lista o que cobra
#   ./scripts/derruba-tudo.sh --sim             # nao pergunta nada
#
# A ordem importa. O Terraform do banco le o state da rede, entao ele sai
# primeiro; destruir a rede antes deixaria o banco orfao, sem como resolver as
# referencias. E as funcoes Lambda nao estao no Terraform - sao publicadas pelo
# pipeline -, entao `terraform destroy` nao as remove. Este script remove.

if [ -z "${BASH_VERSION:-}" ]; then exec bash "$0" "$@"; fi
set -euo pipefail

DIR_SCRIPT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=comum.sh
source "$DIR_SCRIPT/comum.sh"

AMBIENTE="dev"
REGIAO="${AWS_REGION:-us-east-1}"
COM_BOOTSTRAP=0
SO_CONFERIR=0
SEM_PERGUNTAR=0

while [ $# -gt 0 ]; do
  case "$1" in
    --ambiente)      AMBIENTE="$2"; shift 2 ;;
    --com-bootstrap) COM_BOOTSTRAP=1; shift ;;
    --so-conferir)   SO_CONFERIR=1; shift ;;
    --sim)           SEM_PERGUNTAR=1; shift ;;
    -h|--help)       sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) vermelho "Argumento desconhecido: $1"; exit 1 ;;
  esac
done

RAIZ="$(descobrir_raiz)"
K8S="$RAIZ/tech-challenge-infra-k8s"
BANCO="$RAIZ/tech-challenge-infra-database"

# ================================================== varredura do que cobra

# Lista por tipo de recurso, nao por tag: o objetivo e pegar tambem o que ficou
# para tras de um destroy pela metade, que provavelmente perdeu as tags.
conferir_custos() {
  local achou=0

  local clusters
  clusters="$(aws eks list-clusters --query 'clusters' --output text 2>/dev/null || echo '')"
  if [ -n "$clusters" ] && [ "$clusters" != "None" ]; then
    vermelho "  cluster EKS ativo:      $clusters   (~US\$ 2,40/dia cada)"; achou=1
  fi

  local nats
  nats="$(aws ec2 describe-nat-gateways \
    --filter 'Name=state,Values=available,pending' \
    --query 'NatGateways[].NatGatewayId' --output text 2>/dev/null || echo '')"
  if [ -n "$nats" ] && [ "$nats" != "None" ]; then
    vermelho "  NAT Gateway ativo:      $nats   (~US\$ 1,08/dia cada)"; achou=1
  fi

  local dbs
  dbs="$(aws rds describe-db-instances \
    --query 'DBInstances[].DBInstanceIdentifier' --output text 2>/dev/null || echo '')"
  if [ -n "$dbs" ] && [ "$dbs" != "None" ]; then
    vermelho "  RDS ativo:              $dbs   (~US\$ 0,90/dia cada)"; achou=1
  fi

  local lbs
  lbs="$(aws elbv2 describe-load-balancers \
    --query 'LoadBalancers[].LoadBalancerName' --output text 2>/dev/null || echo '')"
  if [ -n "$lbs" ] && [ "$lbs" != "None" ]; then
    vermelho "  balanceador ativo:      $lbs   (~US\$ 0,54/dia cada)"; achou=1
  fi

  # EIP solto cobra justamente por estar solto. E o restinho classico de um
  # destroy interrompido: o NAT sai e o IP fica.
  local eips
  eips="$(aws ec2 describe-addresses \
    --query 'Addresses[?AssociationId==null].PublicIp' --output text 2>/dev/null || echo '')"
  if [ -n "$eips" ] && [ "$eips" != "None" ]; then
    vermelho "  IP elastico SEM USO:    $eips   (cobra por estar ocioso)"; achou=1
  fi

  local instancias
  instancias="$(aws ec2 describe-instances \
    --filters 'Name=instance-state-name,Values=running' \
    --query 'Reservations[].Instances[].InstanceId' --output text 2>/dev/null || echo '')"
  if [ -n "$instancias" ] && [ "$instancias" != "None" ]; then
    amarelo "  EC2 rodando:            $instancias"; achou=1
  fi

  if [ "$achou" -eq 0 ]; then
    verde "  Nada cobrando por hora nesta conta."
  fi
  return 0
}

# ================================================================== inicio

titulo "Verificacoes"
exigir_repos "$RAIZ"
exigir_ferramentas aws terraform
exigir_credencial

CONTA="$(conta_atual)"
BUCKET="$(nome_do_bucket)"
cinza "  conta final:  ...$(echo "$CONTA" | tail -c 5)"
cinza "  ambiente:     $AMBIENTE"

if [ "$SO_CONFERIR" -eq 1 ]; then
  titulo "Recursos que cobram por hora"
  conferir_custos
  exit 0
fi

titulo "O que existe agora"
conferir_custos

avisar_se_a_branch_diverge "$RAIZ" "$AMBIENTE" "DESTRUIR"

if [ "$SEM_PERGUNTAR" -eq 0 ]; then
  echo
  amarelo "Isto DESTROI o ambiente '$AMBIENTE' inteiro, sem volta:"
  echo "  cluster, nodes, NAT, balanceador, banco, gateway, secrets, imagens"
  [ "$COM_BOOTSTRAP" -eq 1 ] && vermelho "  e TAMBEM o bucket de state, com todo o historico"
  echo
  confirmar "Destruir?" || { echo "Cancelado."; exit 0; }
fi

TOTAL=4
[ "$COM_BOOTSTRAP" -eq 1 ] && TOTAL=5

# ---------------------------------------------------------------- lambdas
#
# Primeiro, e nao por acaso: elas nao estao no Terraform, entao ninguem mais as
# remove. Deixar para depois significa esquece-las.

titulo "Destruindo"
etapa 1 "$TOTAL" "Funcoes Lambda"
for f in "tc-grupo160-auth-${AMBIENTE}" "tc-grupo160-authorizer-${AMBIENTE}"; do
  if aws lambda get-function --function-name "$f" >/dev/null 2>&1; then
    aws lambda delete-function --function-name "$f" >/dev/null
    cinza "  removida: $f"
  else
    cinza "  nao existia: $f"
  fi
done

# ------------------------------------------------------------------ banco
#
# Antes da rede: o Terraform do banco le vpc_id e security group do state da
# rede. Se a rede sair primeiro, o destroy do banco falha ao resolver as
# referencias e a instancia fica para tras, cobrando.

etapa 2 "$TOTAL" "Banco de dados"
if tf_init "$BANCO" "$AMBIENTE/banco.tfstate" "$BUCKET" "$REGIAO" 2>/dev/null; then
  terraform -chdir="$BANCO" destroy -auto-approve -input=false \
    -var-file="inventories/$AMBIENTE/terraform.tfvars"
  verde "  Banco destruido."
else
  amarelo "  Sem state de banco para $AMBIENTE - nada a fazer."
fi

# ------------------------------------------------------------------- rede

etapa 3 "$TOTAL" "Cluster, rede, gateway e balanceador"
if tf_init "$K8S/infra" "$AMBIENTE/rede.tfstate" "$BUCKET" "$REGIAO" 2>/dev/null; then
  terraform -chdir="$K8S/infra" destroy -auto-approve -input=false \
    -var-file="inventories/$AMBIENTE/terraform.tfvars"
  verde "  Ambiente $AMBIENTE destruido."
else
  amarelo "  Sem state de rede para $AMBIENTE - nada a fazer."
fi

# -------------------------------------------------------------- bootstrap

if [ "$COM_BOOTSTRAP" -eq 1 ]; then
  etapa 4 "$TOTAL" "Backend de state"
  # force_destroy ja esta ligado no bucket, entao as versoes antigas nao travam
  # a remocao. Ver bootstrap/main.tf.
  terraform -chdir="$K8S/bootstrap" init -input=false >/dev/null
  terraform -chdir="$K8S/bootstrap" destroy -auto-approve -input=false
  verde "  Bucket e tabela de lock removidos."
  PASSO=5
else
  PASSO=4
fi

# ============================================================= conferencia

etapa "$PASSO" "$TOTAL" "Conferindo se sobrou algo cobrando"
sleep 5
titulo "Depois do destroy"
conferir_custos

echo
cinza "Se ainda aparecer algo acima, pode ser de outro ambiente. Rode este"
cinza "script para cada um: --ambiente dev, hom e prod."
