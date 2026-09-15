# Ciclo de vida da infraestrutura

Como subir o ambiente inteiro do zero, derrubá-lo por completo, e mover o
projeto para outra conta da AWS trocando apenas a credencial.

> **O cluster cobra sozinho.** O control plane do EKS custa US$ 0,10/hora
> **enquanto existir** e não é suspenso junto com a sessão do Learner Lab —
> diferente das instâncias EC2. Somando o NAT, o balanceador e o RDS, um
> ambiente de pé custa cerca de **US$ 5/dia**. Derrube ao terminar.

## Os dois comandos

```bash
bash scripts/sobe-tudo.sh
```

```bash
bash scripts/derruba-tudo.sh
```

No Windows, rode pelo **Git Bash** — as armadilhas de rodar pelo WSL estão no
[README do infra-k8s](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s#rodando-os-scripts-no-windows).

### Parâmetros

Ambos, sem argumento, agem sobre o ambiente **`dev`**.

| Parâmetro | Em qual | O que faz |
|---|---|---|
| `--ambiente <dev\|hom\|prod>` | ambos | Escolhe o ambiente. Padrão `dev` |
| `--sim` | ambos | Não pergunta nada. Para uso em automação |
| `-h`, `--help` | ambos | Mostra o cabeçalho do script |
| `--so-infra` | subir | Para depois da conferência, sem publicar as aplicações. Dispensa Docker, kubectl e .NET |
| `--assumir-pipelines` | subir | Aponta as pipelines do time para a **sua** conta. Ver a seção sobre várias pessoas |
| `--so-conferir` | derrubar | Só lista o que está cobrando. **Não destrói nada** |
| `--com-bootstrap` | derrubar | Remove também o bucket de state, com todo o histórico |

Exemplos:

```bash
bash scripts/derruba-tudo.sh --so-conferir
```

```bash
bash scripts/sobe-tudo.sh --ambiente hom --sim
```

### Quanto tempo leva

Medido em 15/09, num ciclo completo do `dev`:

| Comando | Duração |
|---|---|
| `derruba-tudo.sh` | ~18 min |
| `sobe-tudo.sh`, do zero | ~40 min: ~16 na rede e no cluster, ~16 no RDS, o resto nas Lambdas e na API |

A sessão do Learner Lab dura cerca de **4 horas**, e a credencial morre junto.
Comece com folga. Se ela expirar no meio de um apply, o Terraform pode não
conseguir liberar o lock do state, e a execução seguinte para em
`Error acquiring the state lock` — ver [Quando dá errado](#quando-dá-errado).

## Mais de uma pessoa subindo ambiente

**Leia antes de rodar pela primeira vez.**

Cada membro tem a sua conta do Learner Lab, e portanto o seu bucket de state —
os ambientes são independentes e não se atrapalham. O que **é** compartilhado
são os quatro repositórios da organização.

Dois scripts escrevem neles:

| Script | O que grava | Onde |
|---|---|---|
| `sobe-tudo.sh` | `TF_STATE_BUCKET` | variável dos 4 repositórios |
| `renova-secrets.sh` | credenciais da AWS | secrets dos 4 repositórios |

Se duas pessoas rodarem, **a última ganha**: as pipelines passam a aplicar na
conta dela. Os merges de todo o time vão para o ambiente de uma pessoa só — sem
erro nenhum, e sem ninguém perceber.

Por isso o `sobe-tudo.sh` **não sobrescreve em silêncio**. Se a variável já
apontar para outra conta, ele avisa e não mexe:

```text
As pipelines apontam para OUTRA conta.
  atual:  ...4331
  a sua:  ...6131
Nao mexi. Seu ambiente sobe normalmente, mas os merges do time
continuam aplicando na conta de quem configurou antes.
```

O ambiente da pessoa sobe do mesmo jeito — só as pipelines ficam como estavam.
Para assumir de propósito, `--assumir-pipelines`.

**A combinação que funciona:** uma pessoa dona das pipelines, e as demais
subindo ambiente na própria conta para testar, sem tocar nos repositórios. Quem
for assumir, avise o grupo — senão os merges dos outros começam a aplicar num
lugar que eles não esperam.

> O `renova-secrets.sh` **não** tem essa proteção: ele sempre sobrescreve. É o
> comportamento certo para quem é dono das pipelines, e um tiro no pé para quem
> não é. Rode-o apenas se as pipelines forem suas.

### Pipeline e script no mesmo ambiente

Vale para quem é **dono das pipelines** — quando elas aplicam na sua conta.

Enquanto um `sobe-tudo.sh` ou `derruba-tudo.sh` estiver rodando, **não mergeie
nem abra PR** no `infra-k8s` e no `infra-database`, nem que seja só de
documentação: os workflows não filtram por caminho. Todo push em `develop`,
`homolog` ou `main` desses repositórios aplica um ambiente, e todo PR roda plan
nos três. Plan e apply pegam o lock do state, e o Terraform não espera — quem
chega depois falha na hora. Um push na `develop` no meio de um derruba do `dev`
pode até recriar o ambiente pela pipeline.

Quem sobe ambiente na **própria** conta, sem ser dono das pipelines, não tem
esse problema: elas não enxergam a conta dessa pessoa.

## Por que os scripts vivem aqui

Eles orquestram os **quatro** repositórios: leem o Terraform do `infra-k8s`,
aplicam o do `infra-database`, publicam as Lambdas do `lambda-auth` e constroem
a imagem daqui.

Um repositório de infraestrutura dirigindo os outros três inverteria a
separação que o [ADR-0001](adrs/0001-segregacao-em-quatro-repositorios.md)
criou — os quatro são pares, nenhum é dono dos demais.

Este repositório já é o ponto de coordenação do projeto: guarda os ADRs, as
RFCs, a matriz de autorização, o board e todas as issues. Um runbook que
atravessa fronteiras pertence ao mesmo lugar que a documentação que atravessa
fronteiras.

O `renova-secrets.sh` veio junto pelo mesmo motivo. Ele também mexe nos quatro,
e deixá-lo para trás espalharia a ferramenta operacional entre dois
repositórios — pior que qualquer das duas escolhas.

## Pré-requisitos

Os **quatro repositórios lado a lado**, na mesma pasta:

```bash
mkdir -p ~/source/repos/tech-challenge && cd ~/source/repos/tech-challenge
for r in oficina-mecanica lambda-auth infra-k8s infra-database; do
  git clone https://github.com/tech-challenge-grupo-160/tech-challenge-$r.git
done
```

Os scripts descobrem os outros três a partir da própria localização — não há
variável de ambiente para configurar.

**Os scripts usam o código que está na pasta.** O clone traz a branch padrão
(`master` no repositório principal, `main` nos outros), que é a versão entregue.
Para subir o que está em desenvolvimento, faça `git switch develop` nos quatro.
Com os clones em `master`/`main`, o script avisa que a branch "sugere `prod`" —
é esperado: o ambiente é sempre o do `--ambiente`.

| Ferramenta | Para quê |
|---|---|
| AWS CLI v2, Terraform | sempre |
| kubectl, Docker, .NET SDK 10 | publicar as aplicações |
| Node.js LTS (`npx`) | instrumentar as Lambdas com o `datadog-ci`, quando o Datadog está ligado |
| gh | atualizar `TF_STATE_BUCKET` nos repositórios (opcional) |
| Helm | destravar o release do Datadog se um apply for interrompido (opcional) |

Com `--so-infra`, apenas AWS CLI e Terraform bastam.

> **Confira o `npx` antes de começar.** O `sobe-tudo.sh` só procura por ele na
> etapa 6, depois de uns 30 minutos criando infraestrutura. No Windows, depois
> de instalar o Node.js, abra um terminal novo: o `npx` só entra no PATH dos
> terminais abertos depois da instalação.
>
> ```bash
> npx --version
> ```

### Chave do Datadog

Com `datadog_enabled = true` no inventory — o padrão nos três ambientes —, a
subida precisa da chave da API do Datadog. A mesma chave acaba em quatro
lugares:

| Onde | Quem grava | Quem usa |
|---|---|---|
| Variável `TF_VAR_datadog_api_key`, no seu terminal | você | o Terraform, no apply local |
| Segredo `tc-grupo160/<ambiente>/datadog-api-key`, no Secrets Manager | o apply do `infra-k8s`, com o valor da variável | o `sobe-tudo.sh`, quando a variável não foi exportada; as Lambdas instrumentadas |
| Secret `datadog` do Kubernetes, no namespace `datadog` | o Helm, no mesmo apply | o Datadog Agent |
| Secret `TF_VAR_DATADOG_API_KEY` do GitHub, no `infra-k8s` | quem é dono das pipelines | o apply pelas pipelines |

#### 1. Conseguir a chave

A organização do grupo no Datadog fica no site **US1** (`datadoghq.com`). A
chave está em **Organization Settings → API Keys**
(<https://app.datadoghq.com/organization-settings/api-keys>).

Sem acesso à organização, peça a chave a quem a administra — pessoalmente ou
por gerenciador de senhas, nunca por chat. O secret do GitHub não serve de fonte:
ele não pode ser lido de volta.

#### 2. Carregar no terminal sem deixar rastro

```bash
read -rsp "Chave do Datadog: " TF_VAR_datadog_api_key && echo && export TF_VAR_datadog_api_key
```

O `read -s` não mostra o que é digitado e não grava a chave no histórico do
shell — um `export TF_VAR_datadog_api_key="..."` digitado direto gravaria. A
variável vale só para esse terminal: rode o `sobe-tudo.sh` nele.

#### 3. Validar a chave no Datadog

```bash
curl -s -o /dev/null -w '%{http_code}\n' -H "DD-API-KEY: $TF_VAR_datadog_api_key" https://api.datadoghq.com/api/v1/validate
```

`200` é chave válida. `403` é chave errada — ou de outro site do Datadog: a do
grupo responde 403 em `us5`, `eu` e nos demais. Subir com ela deixa o Agent em
`2/3` e faz o Helm estourar o timeout de 15 minutos.

#### 4. Subir

```bash
bash scripts/sobe-tudo.sh --ambiente dev
```

O apply cria o segredo `tc-grupo160/dev/datadog-api-key` com a chave da
variável, e o Helm instala o Agent com ela. Enquanto o ambiente existir, as
próximas subidas nem precisam da variável: o script lê o segredo.

Sem a chave, o script para antes de qualquer apply:

```text
Datadog ligado em inventories/dev, mas sem chave real.
  Exporte TF_VAR_datadog_api_key ou grave tc-grupo160/dev/datadog-api-key
  no Secrets Manager e rode de novo.
```

> **Numa conta nova, não crie o segredo à mão**, apesar do que a mensagem
> sugere. Quem cria o `tc-grupo160/<ambiente>/datadog-api-key` é o Terraform: se
> ele já existir fora do state, o apply falha com `ResourceExistsException`.
> Numa conta nova, ou depois de um `derruba-tudo.sh`, use a variável.

#### Conferir o que está gravado, sem exibir a chave

Compare o hash do segredo com o da chave no seu terminal — iguais, é a mesma
chave:

```bash
aws secretsmanager get-secret-value --secret-id tc-grupo160/dev/datadog-api-key --region us-east-1 --query SecretString --output text | tr -d '\r\n' | sha256sum
```

```bash
printf %s "$TF_VAR_datadog_api_key" | sha256sum
```

Ou valide o segredo direto no Datadog:

```bash
curl -s -o /dev/null -w '%{http_code}\n' -H "DD-API-KEY: $(aws secretsmanager get-secret-value --secret-id tc-grupo160/dev/datadog-api-key --region us-east-1 --query SecretString --output text | tr -d '\r\n')" https://api.datadoghq.com/api/v1/validate
```

E o Agent no cluster, que só fica `3/3` com a chave aceita:

```bash
kubectl get pods -n datadog
```

#### Trocar a chave

Com o ambiente de pé, a chave nova precisa chegar aos quatro lugares da tabela:

1. Carregue a nova (passo 2) e valide (passo 3).
2. Rode `bash scripts/sobe-tudo.sh --ambiente <ambiente>`. A variável exportada
   tem prioridade sobre o segredo: o apply atualiza o Secrets Manager e o Helm
   atualiza o Agent. As Lambdas leem o segredo ao inicializar, então passam a
   usar a nova nas próximas execuções.
3. Se as pipelines forem suas, atualize também o secret do GitHub. Sem isso, o
   próximo merge no `infra-k8s` devolve a chave antiga ao Secrets Manager e ao
   Agent:

   ```bash
   printf %s "$TF_VAR_datadog_api_key" | gh secret set TF_VAR_DATADOG_API_KEY -R tech-challenge-grupo-160/tech-challenge-infra-k8s
   ```

Para trocar **só** o valor do segredo, sem apply, existe o `put-secret-value`.
Ele atualiza o que as Lambdas leem, mas não o Agent do cluster, que só muda num
apply:

```bash
aws secretsmanager put-secret-value --secret-id tc-grupo160/dev/datadog-api-key --region us-east-1 --secret-string "$TF_VAR_datadog_api_key"
```

#### Secret das pipelines

O apply pela pipeline do `infra-k8s` usa o secret `TF_VAR_DATADOG_API_KEY` do
repositório, e só quem é dono das pipelines deve gravá-lo. A partir do segredo
de um ambiente que já está de pé, sem a chave passar pela tela:

```bash
aws secretsmanager get-secret-value --secret-id tc-grupo160/dev/datadog-api-key --region us-east-1 --query SecretString --output text | tr -d '\r\n' | gh secret set TF_VAR_DATADOG_API_KEY -R tech-challenge-grupo-160/tech-challenge-infra-k8s
```

Para conferir, `gh secret list` mostra a data da última gravação — o valor
nunca é exibido:

```bash
gh secret list -R tech-challenge-grupo-160/tech-challenge-infra-k8s
```

#### Depois de derrubar

O `derruba-tudo.sh` destrói o segredo junto com o ambiente. Na subida seguinte,
repita os passos 2 a 4.

## Subindo

O `sobe-tudo.sh` executa oito etapas, nesta ordem:

| # | Etapa | Por que nesta posição |
|---|---|---|
| 1 | Backend de state (S3 + DynamoDB) | tudo mais guarda state nele |
| 2 | **Funções Lambda (código)** | o gateway não sobe sem elas |
| 3 | Rede, cluster, ECR, gateway, ALB, Datadog Agent | base de todo o resto |
| 4 | Banco gerenciado | lê a rede pelo state remoto |
| 5 | Conferência | para cedo se o cluster estiver desligado |
| 6 | Rede e variáveis das funções, instrumentação do Datadog | dependem de subnets, SGs e segredos |
| 7 | API no cluster | precisa do cluster e do ECR |
| 8 | Teste de fumaça | `POST /auth` deve responder 200 |

Duas dessas posições **não são preferência**.

**As funções vêm antes da rede (2 antes de 3).** A `aws_lambda_permission` do
gateway exige que a função exista: a API `AddPermission` devolve 404 se ela não
estiver publicada, e o apply inteiro falha. Num ambiente já em uso o erro nunca
aparece, porque as funções foram publicadas muito antes — numa conta do zero,
aparece sempre.

Só o **código** entra na etapa 2. Rede, variáveis de ambiente e VPC ficam para a
etapa 6, quando subnets, security groups e segredos já existem.

**O banco vem depois da rede (4 depois de 3).** O Terraform do banco lê
`vpc_id`, subnets e o security group do state da rede. Invertendo, ele falha
procurando um state que ainda não existe.

Na etapa 6, o `datadog-ci` imprime `❌ Configuration error: Missing DD_API_KEY`.
É só aviso: a chave chega às funções pelo ARN do segredo, e a instrumentação
acontece mesmo assim.

### Se o cluster não subir

Se `criar_cluster` estiver `false` no inventory do ambiente, o script avisa e
para depois da etapa 4. Sem cluster não há onde publicar a aplicação. Para
ligar, edite `tech-challenge-infra-k8s/infra/inventories/<ambiente>/terraform.tfvars`.

### Conferindo que subiu

O script termina imprimindo o endereço do gateway **com o nome do ambiente no
final**:

```text
Gateway:  https://<id>.execute-api.us-east-1.amazonaws.com/dev
```

Use esse endereço inteiro como base. Sem o `/dev` no caminho, o gateway responde
`404 {"message":"Not Found"}` — até no `/health/live`.

```bash
GATEWAY="https://<id>.execute-api.us-east-1.amazonaws.com/dev"
curl -s -o /dev/null -w '%{http_code}\n' "$GATEWAY/health/live"
curl -s -X POST "$GATEWAY/auth" -H 'Content-Type: application/json' -d '{"documento":"476.548.668-01"}'
```

O CPF acima é o do cliente de teste criado pelas migrations. O `/health/live`
deve responder 200 e o `/auth` deve devolver um `token`. Com ele, uma rota
protegida responde 401 sem o token e 200 com ele:

```bash
TOKEN="<token da resposta anterior>"
curl -s -o /dev/null -w '%{http_code}\n' "$GATEWAY/api/v1/clientes/documento/47654866801"
curl -s -o /dev/null -w '%{http_code}\n' -H "Authorization: Bearer $TOKEN" "$GATEWAY/api/v1/clientes/documento/47654866801"
```

Perdeu o endereço? O nome da API no gateway é `tc-grupo160-<ambiente>`:

```bash
aws apigatewayv2 get-apis --query "Items[?Name=='tc-grupo160-dev'].ApiEndpoint" --output text
```

E acrescente `/dev` ao final. O Datadog Agent está saudável quando os pods do
DaemonSet aparecem com `3/3`:

```bash
kubectl get pods -n datadog
```

## Derrubando

O `derruba-tudo.sh` desfaz na ordem inversa, e a inversão importa:

1. **Funções Lambda** — primeiro, e não por acaso
2. **Banco** — antes da rede, senão o destroy não resolve as referências
3. **Cluster, rede, gateway, balanceador**
4. **Backend de state** — só com `--com-bootstrap`
5. **Conferência** do que ficou cobrando

> **O banco sai sem deixar cópia.** A instância é criada com
> `skip_final_snapshot = true`, então o passo 2 apaga os dados e pronto. Se
> houver algo no RDS que você queira de volta, tire um snapshot manual **antes**
> de rodar o script — o procedimento está em
> [ROLLBACK-BANCO.md](ROLLBACK-BANCO.md).

> **A chave do Datadog sai junto.** O segredo
> `tc-grupo160/<ambiente>/datadog-api-key` é destruído com o ambiente. Na
> próxima subida, carregue a chave de novo — passos 2 a 4 de
> [Chave do Datadog](#chave-do-datadog).

### Por que as Lambdas saem primeiro

**Elas não estão no Terraform.** São publicadas pelo pipeline com
`dotnet lambda deploy-function`, porque gerenciá-las nos dois lugares faria o
Terraform e o pipeline disputarem o mesmo recurso.

A consequência é que `terraform destroy` **não as remove**. Deixá-las para o
fim significa esquecê-las. Elas não cobram por hora, mas ficam para trás
apontando para uma VPC que não existe mais.

### A conferência de custos

A última etapa varre a conta procurando o que cobra por hora, **por tipo de
recurso e não por tag** — o objetivo é pegar também o que sobrou de um destroy
interrompido, que provavelmente perdeu as tags:

- clusters EKS
- NAT Gateways
- instâncias RDS
- balanceadores
- **IPs elásticos sem uso** — o restinho clássico: o NAT sai e o IP fica, cobrando justamente por estar ocioso
- instâncias EC2 rodando

Dá para rodar só a varredura, sem destruir nada:

```bash
bash scripts/derruba-tudo.sh --so-conferir
```

### Ambientes são independentes

O script derruba **um ambiente**. Se `hom` ou `prod` também estiverem de pé,
rode para cada um.

### O ambiente não vem da branch

Existem **exatamente três** ambientes — `dev`, `hom` e `prod` —, cada um com seu
próprio state e seus próprios recursos na AWS. Uma branch de feature **não ganha
ambiente próprio**: ela compartilha o `dev`, que é aplicado quando algo entra na
`develop`.

Os scripts escolhem o ambiente pelo `--ambiente`, e **nunca olham para a
branch**. O padrão é `dev`. Derivar da branch faria o mesmo comando se comportar
de formas diferentes sem mudar — o tipo de mágica que destrói o ambiente errado
sem ninguém entender por quê.

A ligação branch → ambiente existe só nas pipelines:

| Branch | Pipeline aplica |
|---|---|
| `develop` | `dev` |
| `homolog` | `hom` |
| `main` / `master` | `prod` |

Como o padrão silencioso enfraquece a escolha explícita — quem trabalha em
`homolog` e roda sem argumento derrubaria o `dev` —, os scripts avisam quando a
branch de algum repositório sugere outro ambiente:

```text
ATENCAO: a branch de algum repositorio sugere outro ambiente.
  tech-challenge-infra-k8s: homolog (a pipeline aplicaria 'hom')

O comando vai DESTRUIR o ambiente 'dev'.
```

O aviso não bloqueia: informa antes da confirmação. Explícito continua
explícito, e o engano fica difícil de cometer em silêncio.

> Uma consequência de os ambientes não serem por branch: **aplicar código de
> branch não mergeada não sobrevive**. O próximo push na `develop` reconcilia o
> `dev` com o que está lá e desfaz o que não estiver no código. Já derrubou um
> authorizer no meio do caminho.

## Mudando de conta AWS

O projeto é portável entre contas do Learner Lab por construção, não por
esforço:

| O que poderia amarrar | Como é resolvido |
|---|---|
| Nome do bucket de state | `tc-grupo160-tfstate-<id-da-conta>`, derivado em tempo de execução |
| ARNs da LabRole e das Lambdas | montados com `data.aws_caller_identity` |
| State remoto entre repositórios | o `infra-database` monta o nome do bucket do mesmo jeito |
| State local do bootstrap | fora do Git, e o script move o de outra conta para `.antigo` |

O procedimento é:

1. Cole a credencial da conta nova em `~/.aws/credentials`, perfil `[default]`
2. Carregue a chave do Datadog no terminal — numa conta nova o segredo ainda não
   existe. Passos 1 a 3 de [Chave do Datadog](#chave-do-datadog)
3. `bash scripts/sobe-tudo.sh`

Se usar as pipelines, rode também:

```bash
bash scripts/renova-secrets.sh
```

O `sobe-tudo.sh` já atualiza a variável `TF_STATE_BUCKET` nos quatro
repositórios quando o `gh` está autenticado — sem isso as pipelines apontariam
para o bucket da conta antiga.

> **Só funciona em contas do AWS Academy Learner Lab.** Todo o Terraform
> referencia a `LabRole`, que só existe lá. Numa conta AWS comum seria preciso
> criar as roles — o que o lab proíbe e por isso o projeto nunca fez. Ver a
> RFC-0001 e a issue #59.

## Quando dá errado

| Sintoma | Causa | Saída |
|---|---|---|
| `a credencial existe mas esta sendo negada` | sessão do lab encerrada (`voc-cancel-cred`) | Start Lab e cole a credencial nova |
| `Error acquiring the state lock` | outro plan ou apply no mesmo ambiente — inclusive pipeline disparada por merge ou PR —, ou sessão que expirou no meio de um apply | espere o que estiver rodando; só use `terraform force-unlock <ID>` se tiver certeza de que nada está rodando |
| `Datadog ligado em inventories/<ambiente>, mas sem chave real` | conta nova, ou subida logo depois de um derruba | carregue a chave ([Chave do Datadog](#chave-do-datadog), passos 2 e 3) e rode de novo |
| `ResourceExistsException` ao criar `tc-grupo160/<ambiente>/datadog-api-key` | segredo criado à mão antes do primeiro apply | `aws secretsmanager delete-secret --secret-id tc-grupo160/<ambiente>/datadog-api-key --force-delete-without-recovery` e suba com a variável |
| `helm_release.datadog_agent`: `context deadline exceeded` | o Agent não consegue falar com o Datadog: chave inválida ou dummy, ou `datadog_site` diferente de `datadoghq.com` | corrija a chave ou o site. Se o release ficou travado, `aws eks update-kubeconfig --name tc-grupo160-<ambiente>`, `helm uninstall datadog -n datadog` e rode de novo |
| `ERRO: 'npx' nao encontrado no PATH` na etapa 6 | Node.js ausente, ou terminal aberto antes da instalação | instale o Node.js LTS, abra um terminal novo e rode de novo |
| `404 {"message":"Not Found"}` no `/auth` ou no `/health/live` | endereço sem o nome do ambiente | use `https://<id>.execute-api.us-east-1.amazonaws.com/<ambiente>/auth` |
| `POST /auth` devolve 503 logo após subir | VPC Link ainda propagando | espere ~3 min |
| `POST /auth` devolve 500 e a Lambda não registra invocação | o gateway perdeu a permissão de invocar a Lambda: apply do `infra-k8s` com `lambdas_publicadas = false` | aplique de novo sem essa variável — o padrão é `true` |
| Pipeline de infra falha com 403 ao ler o `.tfstate` | secrets com credencial de sessão encerrada ou de outra conta | quem é dono das pipelines roda `renova-secrets.sh` |
| Nodes ficam `NotReady` | subnet privada sem saída | confirme que o NAT Gateway existe |
| Destroy da rede falha citando o authorizer | rota ainda referencia o authorizer | rode o destroy de novo; a ordem se resolve na segunda passada |
| Sobrou EIP na conferência | destroy interrompido | `aws ec2 release-address --allocation-id <id>` |

## O que os scripts não fazem

**Não promovem branches.** Subir infraestrutura e promover código são coisas
separadas; a promoção continua sendo por PR.

**Não criam o board nem as issues.**

**Não cuidam de `hom` e `prod` juntos.** Um ambiente por execução, de propósito
— derrubar três ambientes por engano com um comando seria fácil demais.

**Não protegem a instrumentação do Datadog das pipelines.** O `sobe-tudo.sh`
instrumenta as Lambdas com o `datadog-ci`. O deploy das Lambdas pela pipeline
substitui as variáveis de ambiente das funções e leva junto as do Datadog, e os
traces das Lambdas param. Para instrumentar de novo, rode o `sobe-tudo.sh` no
ambiente.
