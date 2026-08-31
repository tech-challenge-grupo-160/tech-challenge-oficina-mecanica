#!/usr/bin/env bash
#
# Funcoes compartilhadas por sobe-tudo.sh e derruba-tudo.sh.
#
# Nao roda sozinho: e carregado com `source`.

# ------------------------------------------------------------------- cores

vermelho() { printf '\033[31m%s\033[0m\n' "$1"; }
verde()    { printf '\033[32m%s\033[0m\n' "$1"; }
amarelo()  { printf '\033[33m%s\033[0m\n' "$1"; }
cinza()    { printf '\033[90m%s\033[0m\n' "$1"; }

titulo() {
  echo
  printf '\033[1m%s\033[0m\n' "$1"
  printf '\033[90m%s\033[0m\n' "$(printf '%.0s-' $(seq 1 ${#1}))"
}

etapa()  { printf '\033[36m[%s/%s]\033[0m %s\n' "$1" "$2" "$3"; }

# ---------------------------------------------------------------- caminhos
#
# Os quatro repositorios ficam lado a lado. Este script vive em
# tech-challenge-oficina-mecanica/scripts, entao dois niveis acima e a pasta que os
# contem. Derivar em vez de exigir variavel evita que o script funcione so na
# maquina de quem o escreveu.

descobrir_raiz() {
  local aqui
  aqui="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
  echo "$aqui"
}

exigir_repos() {
  local raiz="$1"
  local faltando=0
  local r
  for r in tech-challenge-infra-k8s tech-challenge-infra-database \
           tech-challenge-lambda-auth tech-challenge-oficina-mecanica; do
    if [ ! -d "$raiz/$r" ]; then
      vermelho "ERRO: repositorio ausente: $raiz/$r"
      faltando=1
    fi
  done
  if [ "$faltando" -eq 1 ]; then
    echo
    echo "Os quatro repositorios precisam estar lado a lado:"
    echo
    echo "  mkdir -p ~/source/repos/tech-challenge && cd ~/source/repos/tech-challenge"
    echo "  for r in oficina-mecanica lambda-auth infra-k8s infra-database; do"
    echo "    git clone https://github.com/tech-challenge-grupo-160/tech-challenge-\$r.git"
    echo "  done"
    exit 1
  fi
}

# ------------------------------------------------------------- ferramentas

exigir_ferramentas() {
  local faltando=0
  local f
  for f in "$@"; do
    if ! command -v "$f" >/dev/null 2>&1; then
      vermelho "ERRO: '$f' nao encontrado no PATH."
      faltando=1
    fi
  done
  [ "$faltando" -eq 0 ] || exit 1
}

# ------------------------------------------------------------- credenciais

# O get-caller-identity responde mesmo com a sessao do lab encerrada; o que
# denuncia a credencial morta e um deny explicito em qualquer outra chamada.
# Por isso a checagem faz as duas coisas.
#
# A segunda chamada NAO leva --max-items: describe-availability-zones nao e uma
# operacao paginada, e o CLI rejeita o argumento com erro de uso. Como a saida
# ia para /dev/null, o script culpava a credencial por um erro de sintaxe e
# mandava reiniciar o lab que estava perfeitamente bem. Corrigido em 30/08.
#
# Por isso tambem a saida e lida antes de acusar: so o texto da resposta
# distingue "sem permissao" de "o comando quebrou por outro motivo". Errar essa
# distincao manda a pessoa procurar no lugar errado.
exigir_credencial() {
  if ! aws sts get-caller-identity >/dev/null 2>&1; then
    vermelho "ERRO: credencial da AWS invalida ou ausente."
    echo "Start Lab no painel do AWS Academy, depois AWS Details -> AWS CLI,"
    echo "e cole o bloco em ~/.aws/credentials no perfil [default]."
    exit 1
  fi

  local saida
  if saida="$(aws ec2 describe-availability-zones \
       --query 'AvailabilityZones[0].ZoneName' --output text 2>&1)"; then
    return 0
  fi

  if echo "$saida" | grep -qiE "explicit deny|AccessDenied|UnauthorizedOperation|ExpiredToken|InvalidClientTokenId"; then
    vermelho "ERRO: a credencial existe mas esta sendo negada."
    echo "Isso costuma ser a policy 'voc-cancel-cred': a sessao do lab encerrou."
    echo "Start Lab de novo e cole a credencial nova."
  else
    vermelho "ERRO: nao consegui falar com a AWS, e nao parece ser a credencial."
    echo "A resposta foi:"
    echo "$saida" | sed 's/^/  /' | head -5
  fi
  exit 1
}

conta_atual() { aws sts get-caller-identity --query Account --output text; }

# O Terraform referencia a LabRole em todos os componentes. Sem ela, o apply
# falha no meio - melhor descobrir antes de criar meia infraestrutura.
exigir_lab_role() {
  if ! aws iam get-role --role-name LabRole >/dev/null 2>&1; then
    amarelo "AVISO: nao consegui confirmar a LabRole nesta conta."
    echo "  O lab nega iam:GetRole em algumas sessoes, entao isso pode ser"
    echo "  falso alarme. Mas se a conta nao for um AWS Academy Learner Lab,"
    echo "  a LabRole nao existe e o apply vai falhar."
    echo
    return 1
  fi
  return 0
}

# ------------------------------------------------------------------ estado

# O nome do bucket carrega o id da conta, entao trocar de conta troca de bucket
# sem ninguem editar nada. E o que torna o projeto portavel.
nome_do_bucket() { echo "tc-grupo160-tfstate-$(conta_atual)"; }

tf_init() {
  local dir="$1" chave="$2" bucket="$3" regiao="$4"
  terraform -chdir="$dir" init -reconfigure -input=false \
    -backend-config="bucket=$bucket" \
    -backend-config="key=$chave" \
    -backend-config="region=$regiao" \
    -backend-config="dynamodb_table=tc-grupo160-tflock" >/dev/null
}

confirmar() {
  local pergunta="$1"
  local resposta
  printf '%s [digite: sim] ' "$pergunta"
  read -r resposta
  [ "$resposta" = "sim" ]
}
