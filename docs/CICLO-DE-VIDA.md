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

No Windows, rode pelo **Git Bash**.

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

| Ferramenta | Para quê |
|---|---|
| AWS CLI v2, Terraform | sempre |
| kubectl, Docker, .NET SDK 10 | publicar as aplicações |
| gh | atualizar `TF_STATE_BUCKET` nos repositórios (opcional) |

Com `--so-infra`, apenas AWS CLI e Terraform bastam.

## Subindo

O `sobe-tudo.sh` executa sete etapas, nesta ordem:

| # | Etapa | Por que nesta posição |
|---|---|---|
| 1 | Backend de state (S3 + DynamoDB) | tudo mais guarda state nele |
| 2 | **Funções Lambda (código)** | o gateway não sobe sem elas |
| 3 | Rede, cluster, ECR, gateway, ALB | base de todo o resto |
| 4 | Banco gerenciado | lê a rede pelo state remoto |
| 5 | Conferência | para cedo se o cluster estiver desligado |
| 6 | Rede e variáveis das funções | dependem de subnets, SGs e segredos |
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

### Se o cluster não subir

Se `criar_cluster` estiver `false` no inventory do ambiente, o script avisa e
para depois da etapa 4. Sem cluster não há onde publicar a aplicação. Para
ligar, edite `tech-challenge-infra-k8s/infra/inventories/<ambiente>/terraform.tfvars`.

## Derrubando

O `derruba-tudo.sh` desfaz na ordem inversa, e a inversão importa:

1. **Funções Lambda** — primeiro, e não por acaso
2. **Banco** — antes da rede, senão o destroy não resolve as referências
3. **Cluster, rede, gateway, balanceador**
4. **Backend de state** — só com `--com-bootstrap`
5. **Conferência** do que ficou cobrando

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

O procedimento é literalmente:

1. Cole a credencial da conta nova em `~/.aws/credentials`, perfil `[default]`
2. `bash scripts/sobe-tudo.sh`

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
| `Error acquiring the state lock` | outro apply em andamento | espere; só use `force-unlock` se tiver certeza de que nada está rodando |
| `POST /auth` devolve 503 logo após subir | VPC Link ainda propagando | espere ~3 min |
| Nodes ficam `NotReady` | subnet privada sem saída | confirme que o NAT Gateway existe |
| Destroy da rede falha citando o authorizer | rota ainda referencia o authorizer | rode o destroy de novo; a ordem se resolve na segunda passada |
| Sobrou EIP na conferência | destroy interrompido | `aws ec2 release-address --allocation-id <id>` |

## O que os scripts não fazem

**Não promovem branches.** Subir infraestrutura e promover código são coisas
separadas; a promoção continua sendo por PR.

**Não criam o board nem as issues.**

**Não cuidam de `hom` e `prod` juntos.** Um ambiente por execução, de propósito
— derrubar três ambientes por engano com um comando seria fácil demais.
