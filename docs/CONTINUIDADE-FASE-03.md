# Guia de continuidade — Fase 03

Estado do projeto em **24/08/2026** e o que cada pessoa pode pegar a seguir.

> **Faltam 21 dias para a entrega (15/09).** O board tem **17 issues fechadas e 43 abertas**.

---

## 1. Onde estamos

### O que já funciona

| Entrega | Estado |
|---|---|
| 4 repositórios separados, com CI | ✅ |
| Branch principal protegida nos 4 | ✅ 1 aprovação, sem commit direto |
| Lambda de autenticação implementada | ✅ .NET 10, 16 testes, 59% de cobertura |
| Lambda publicada na AWS | ✅ `tc-grupo160-auth-hom` |
| Backend de state remoto com lock | ✅ S3 versionado + DynamoDB |
| Rede AWS (VPC, subnets, SGs) | ✅ aplicada em `dev` |
| Pipelines autenticando na AWS | ✅ credenciais de sessão |
| RFC da nuvem e ADR do split | ✅ mergeadas |

**Épico E2 está 8 de 9.** Falta só a [#52](../../issues/52), que precisa do cluster existir.

### O que existe na AWS hoje

Região `us-east-1` (única permitida pelo Learner Lab):

```
VPC 10.0.0.0/16
├── subnets públicas   10.0.0.0/20, 10.0.16.0/20      (us-east-1a, 1b)
├── subnets privadas   10.0.128.0/20, 10.0.144.0/20   (us-east-1a, 1b)
├── internet gateway + 2 tabelas de rota
└── 4 security groups: alb, nodes, banco, lambda

S3    tc-grupo160-tfstate-<conta>   (state, versionado)
Dynamo tc-grupo160-tflock            (lock)
Lambda tc-grupo160-auth-hom          (dotnet10)
```

**Custo atual: praticamente zero.** Nada aqui cobra por hora. Isso muda quando o EKS entrar.

### O que ninguém fez ainda

**Nenhum código chegou às branches de produção.** Em todos os 4 repositórios, `develop` está de 3 a 8 commits à frente de `main`/`master`. O `apply` automático e o deploy por branch **nunca chegaram a rodar de verdade** — só validamos por `workflow_dispatch`.

Promover ao menos uma vez é importante: valida a automação e é o que o avaliador vai olhar ao abrir o repositório.

---

## 2. Como trabalhar no projeto

### Os quatro repositórios

| Repositório | O que vive nele |
|---|---|
| `tech-challenge-oficina-mecanica` | API .NET, documentação, **o board e todas as issues** |
| `tech-challenge-lambda-auth` | Function de autenticação por CPF |
| `tech-challenge-infra-k8s` | Terraform da rede e do cluster, manifests |
| `tech-challenge-infra-database` | Terraform do banco gerenciado |

Clone os quatro lado a lado:

```bash
mkdir -p ~/source/repos/tech-challenge && cd ~/source/repos/tech-challenge
for r in oficina-mecanica lambda-auth infra-k8s infra-database; do git clone https://github.com/tech-challenge-grupo-160/tech-challenge-$r.git; done
```

### A regra que evita confusão

> **Uma issue → um repositório → um PR.**

As issues vivem todas no repo principal, mas o trabalho acontece nos quatro. Para ligar um PR de outro repositório à issue, use o caminho completo **no corpo do PR**:

```
Refs tech-challenge-grupo-160/tech-challenge-oficina-mecanica#39
```

Para fechar automaticamente ao mergear, troque `Refs` por `Closes`.

### Fluxo de branches

```
feature/* → develop → homolog → main
```

Só a branch de produção (`main`, ou `master` no repo principal) é protegida e exige 1 aprovação. `develop` e `homolog` aceitam push direto — **use PR mesmo assim**, porque é o que dispara o CI.

Ao promover entre branches de vida longa, use **merge commit, nunca squash**:

```bash
gh pr merge --merge
```

Squash cria commits novos e faz `develop` e `main` divergirem para sempre, gerando conflitos que não deveriam existir.

### Credenciais da AWS — toda sessão

As credenciais do AWS Academy Learner Lab **expiram a cada ~4 horas**. Quando o pipeline falhar com `403` ou `ExpiredToken`, é isso.

No painel do lab: **AWS Details → AWS CLI**. Cole o bloco no `~/.aws/credentials` (Windows: `C:\Users\<voce>\.aws\credentials`), no perfil `[default]`, sem apagar outros perfis que existirem.

Depois atualize os secrets do GitHub:

```bash
bash tech-challenge-infra-k8s/scripts/renova-secrets.sh
```

No Windows, via Git Bash:

```powershell
& "C:\Program Files\Git\bin\bash.exe" scripts/renova-secrets.sh
```

Para só conferir o que está cadastrado, sem alterar:

```bash
bash scripts/renova-secrets.sh --check
```

> **Credencial não passa por chat, e-mail ou grupo de mensagem** — nem as temporárias. Se uma vazar: **End Lab** e **Start Lab** invalidam a sessão na hora.

### Trocar credenciais vs. trocar de conta

São situações muito diferentes. Confundir as duas leva a apagar infraestrutura sem necessidade.

#### Credenciais novas, mesma conta — rotina

Acontece a cada sessão do lab, e também quando o professor renova o crédito.

**Não é preciso reaplicar nada.** Credencial é só autenticação. O state guarda **IDs de recursos** (`vpc-0a1b2c...`), e esses recursos continuam existindo. Os recursos de rede não são "ligados": sobrevivem ao lab dormir. O que o lab suspende são instâncias EC2.

Procedimento:

1. Cole o bloco novo no `~/.aws/credentials`, perfil `[default]`
2. Rode `bash scripts/renova-secrets.sh` no `infra-k8s`
3. Siga trabalhando

Se algum pipeline tiver falhado com credencial velha, reexecute sem precisar de commit novo:

```bash
gh run rerun <id-da-execucao> --repo tech-challenge-grupo-160/<repo> --failed
```

#### Conta AWS diferente — recomeço

Se a conta mudar, o state velho **vira lixo**: ele aponta para IDs que não existem na conta nova, e o próprio bucket de state está na conta antiga, inacessível.

**Como detectar.** Rode e compare com o número anterior:

```bash
aws sts get-caller-identity --query Account --output text
```

> Registrem o número da conta atual num lugar acessível ao grupo — **fora de repositório público**. Sem essa referência, ninguém percebe a troca até um `terraform plan` estranho aparecer.

**Procedimento completo:**

1. **Confirme que mudou mesmo.** Compare o número. Renovar crédito **não** troca a conta; resetar o lab pode trocar.

2. **Recrie o backend de state** — o bucket antigo não é mais alcançável:

   ```bash
   cd tech-challenge-infra-k8s/bootstrap
   rm -f terraform.tfstate terraform.tfstate.backup
   terraform init && terraform apply
   ```

   O nome do bucket muda sozinho: ele inclui o id da conta, resolvido em runtime.

3. **Atualize a variável nos dois repositórios de infra:**

   ```bash
   gh variable set TF_STATE_BUCKET --body "$(terraform output -raw state_bucket)" --repo tech-challenge-grupo-160/tech-challenge-infra-k8s
   gh variable set TF_STATE_BUCKET --body "$(terraform output -raw state_bucket)" --repo tech-challenge-grupo-160/tech-challenge-infra-database
   ```

4. **Reinicialize e reaplique cada módulo**, apontando para o backend novo:

   ```bash
   cd ../infra
   rm -rf .terraform
   terraform init -backend-config="bucket=<bucket-novo>" -backend-config="key=dev/rede.tfstate" -backend-config="region=us-east-1" -backend-config="dynamodb_table=tc-grupo160-tflock"
   terraform apply -var-file=inventories/dev/terraform.tfvars
   ```

   O `rm -rf .terraform` é necessário: sem ele o Terraform tenta migrar o state do backend antigo e falha.

5. **Renove os secrets** com `renova-secrets.sh`

6. **Reimplante a Lambda** — ela vive na conta antiga:

   ```bash
   gh workflow run ci.yml --repo tech-challenge-grupo-160/tech-challenge-lambda-auth --ref develop -f ambiente=homologacao
   ```

Nada disso exige mudar código. Todos os recursos são criados pelo Terraform, e o id da conta nunca está escrito nos arquivos — vem sempre de `data.aws_caller_identity`.

#### Conta igual, mas os recursos sumiram

Acontece se a AWS Academy limpar a conta. O state diz que os recursos existem, mas não existem.

O sintoma é o `terraform plan` mostrar **tudo como `will be created`** em um ambiente que você sabe que já foi aplicado.

Não é problema: rode `terraform apply` e ele recria. O state se reconcilia sozinho. Só não se assuste com o tamanho do plano.

### Aplicar infraestrutura

```bash
cd tech-challenge-infra-k8s/infra
terraform init \
  -backend-config="bucket=tc-grupo160-tfstate-<conta>" \
  -backend-config="key=dev/rede.tfstate" \
  -backend-config="region=us-east-1" \
  -backend-config="dynamodb_table=tc-grupo160-tflock"
terraform apply -var-file=inventories/dev/terraform.tfvars
```

No PowerShell, **use aspas no `-var-file`**, senão ele quebra o argumento:

```powershell
terraform apply -var-file="inventories\dev\terraform.tfvars"
```

---

## 3. O que pegar agora

### Pode começar hoje, sem depender de ninguém

Estas issues **não precisam de AWS, credencial ou cluster**. Se você quer contribuir agora, pegue uma daqui:

| Issue | O quê | Por que importa |
|---|---|---|
| [#35](../../issues/35) | RFC da estratégia de autenticação | Entregável avaliado. Bloqueia [#42](../../issues/42) e [#43](../../issues/43) |
| [#66](../../issues/66) | RFC da ferramenta de observabilidade | Entregável avaliado. Bloqueia todo o E4 |
| [#68](../../issues/68) | Logs estruturados em JSON na API | Requisito explícito da fase |
| [#69](../../issues/69) | Correlation ID entre requisições | Requisito explícito da fase |
| [#44](../../issues/44) | Matriz de rotas públicas vs. protegidas | Define o que o authorizer protege |
| [#82](../../issues/82) | Diagrama de sequência da abertura de OS | Entregável avaliado |
| [#76](../../issues/76) | Justificativa da escolha do banco | Entregável avaliado |
| [#78](../../issues/78) | Diagrama ER com relacionamentos | Entregável avaliado |
| [#85](../../issues/85) | Roteiro do vídeo | Define o que precisa estar demonstrável |

**As duas RFCs são as mais urgentes.** Elas eram da Sprint 1, ninguém escreveu, e são itens que o avaliador procura. Use o template em [`docs/rfcs/TEMPLATE.md`](rfcs/TEMPLATE.md) e siga o exemplo da [RFC-0001](rfcs/0001-escolha-da-nuvem.md).

### O caminho crítico

Tudo que falta de infraestrutura passa por aqui, nesta ordem:

```
#60 EKS  →  #52 CD da app (fecha o E2)
         →  #67 agente de observabilidade  →  #70..#74 métricas e dashboards
#61 RDS  →  #62 migração de dados  →  #38 e #39 completam a Lambda
#46 Secrets Manager  →  #39 e #45
```

**O [#60](../../issues/60) é o gargalo.** Enquanto o cluster não existir, o épico de observabilidade inteiro fica parado, e sem dashboards não há o "dashboard com análise ao vivo" que o vídeo precisa mostrar.

### Marcadas como opcionais

Cinco issues têm `[OPCIONAL]` no título: [#63](../../issues/63), [#65](../../issues/65), [#75](../../issues/75), [#77](../../issues/77) e [#79](../../issues/79). Nenhuma é exigida no enunciado. **Corte-as primeiro se o prazo apertar.**

---

## 4. Como continuar cada épico

### E1 — Autenticação (9 abertas)

O núcleo já funciona: validação de CPF, consulta de cliente, geração de JWT e testes, tudo na Lambda em `develop`.

Falta ligar as pontas:

1. **[#35](../../issues/35)** RFC — pode ser feita agora
2. **[#44](../../issues/44)** matriz de autorização — agora
3. **[#46](../../issues/46)** Secrets Manager — depende de nada, é Terraform
4. **[#38](../../issues/38)** e **[#39](../../issues/39)** — completar a Lambda com segredos e banco
5. **[#42](../../issues/42)** API Gateway e **[#43](../../issues/43)** authorizer
6. **[#45](../../issues/45)** API .NET aceitando o JWT da Lambda

> ⚠️ **Pendência de segurança em aberto.** `JwtOptions.SecretKey` tem valor padrão embutido no código, num repositório público. Se a Lambda subir sem `JWT_SECRET_KEY`, ela assina tokens com uma chave publicada no GitHub. `DatabaseOptions`, no mesmo projeto, já faz o certo — lança exceção quando falta. Vale aplicar o mesmo padrão. Detalhes na [#39](../../issues/39).

### E2 — Repositórios e CI/CD (1 aberta)

Só a **[#52](../../issues/52)**, que precisa do EKS. Quando o cluster existir, é ajustar o workflow do repo principal para publicar imagem no ECR e aplicar os manifests.

### E3 — Infraestrutura (6 abertas)

1. **[#60](../../issues/60) EKS** — o passo mais pesado. A rede já tem as tags que o AWS Load Balancer Controller procura
2. **[#61](../../issues/61) RDS** — Single-AZ, no repo `infra-database`. O security group `sg-banco` já existe e só aceita 5432 dos SGs de nodes e Lambda
3. **[#62](../../issues/62)** migrar schema e dados
4. **[#64](../../issues/64)** Ingress e TLS

> 💸 **O EKS cobra US$ 0,10/hora enquanto existir** e **não para quando o lab dorme**, diferente de instâncias EC2. Deixar o cluster de pé por 21 dias consome ~US$ 50 do crédito. **Combinem quem roda `terraform destroy` do cluster ao fim de cada sessão** — recriar leva ~15 minutos, e a rede e o bootstrap podem ficar porque custam zero.

### E4 — Observabilidade (10 abertas)

Comece pela **[#66](../../issues/66)**, a RFC, que decide a ferramenta. Em paralelo, **[#68](../../issues/68)** e **[#69](../../issues/69)** podem ser feitas já — são código na API, não dependem de nuvem.

O resto ([#67](../../issues/67), [#70](../../issues/70)–[#74](../../issues/74)) espera o cluster.

Os três dashboards da [#74](../../issues/74) são **exigidos nominalmente** no enunciado: volume diário de OS, tempo médio por status e erros de integração.

### E5 — Banco de dados (4 abertas)

**[#76](../../issues/76)** e **[#78](../../issues/78)** são entregáveis avaliados e podem ser feitas agora. As outras duas são opcionais.

### E6 — Documentação (3 abertas)

**[#82](../../issues/82)** pode ser feita agora. A **[#80](../../issues/80)** (diagrama de componentes) precisa que EKS e RDS existam para refletir a realidade — o diagrama AWS já está em [`docs/diagrams/C4_04_AWS_Deployment_Diagram.puml`](diagrams/) e serve de base.

A **[#84](../../issues/84)** (READMEs dos 4 repos) é rápida e some no fim se ninguém pegar.

### E7 — Entrega (3 abertas)

**[#85](../../issues/85)** o roteiro do vídeo deveria ser feito **já**, não no fim — ele define o que precisa estar demonstrável e revela cedo o que não vai dar tempo.

O vídeo precisa mostrar: autenticação com CPF, execução da pipeline, deploy automatizado, consumo das APIs protegidas, dashboard com análise ao vivo, e logs e traces em execução.

---

## 5. Armadilhas que já nos custaram tempo

| Sintoma | Causa | Solução |
|---|---|---|
| Pipeline falha com `403` no S3 | Credenciais do lab expiraram | `renova-secrets.sh` |
| `NoCredentials` no `aws` | Não existe perfil `[default]` | Cole o bloco do lab no `~/.aws/credentials` |
| `Unable to parse config file` | Perfil colado por cima do cabeçalho de outro | Cada perfil precisa do seu `[nome]` |
| `Syntax error: "(" unexpected` | Script bash rodado com `sh` (dash) | Use `bash script.sh` |
| `Failed to load ".tfvars" as a plan file` | PowerShell quebrou o argumento | Use aspas: `-var-file="caminho"` |
| `Backend initialization required` | `init` com `-backend=false` antes do `plan` | Já corrigido no CI |
| Job verde mas nada aconteceu | `continue-on-error` mascarando falha | Já removido do `plan` |
| `plan` mostra tudo como `will be created` | Recursos apagados, ou a conta AWS mudou | Ver "Trocar credenciais vs. trocar de conta" |

**A lição que se repetiu:** status verde não é prova de execução. Confira a saída do passo, não o ícone do job.

---

## 6. Decisões já tomadas

Não precisam ser rediscutidas — estão documentadas:

- **Nuvem: AWS** — [RFC-0001](rfcs/0001-escolha-da-nuvem.md)
- **Split em 4 repositórios** — [ADR-0001](adrs/0001-segregacao-em-quatro-repositorios.md)
- **Sem OIDC**, credenciais de sessão como secrets — limitação do Learner Lab, aprovada pelo professor
- **Sem menor privilégio**, `LabRole` compartilhada — limitação do Learner Lab
- **RDS Single-AZ** e **sem NAT Gateway** — decisões de custo
- **Repositórios públicos** — única forma de ter branch protection no plano Free

> As três últimas são **limitações do ambiente, não escolhas de arquitetura**. O PDF de entrega deve declará-las junto com o desenho correto que ficou inviável — isso mostra domínio, e a RFC-0001 já traz o texto.

---

## 7. Antes da entrega

- [ ] Promover `develop → homolog → main` nos 4 repositórios ao menos uma vez
- [ ] Confirmar que `soat-architecture` **aceitou** o convite da organização — em 24/08 ainda estava pendente
- [ ] Tornar os checks de CI obrigatórios na branch de produção (`required_status_checks` está vazio nos 4)
- [ ] Corrigir a chave JWT embutida no código
- [ ] Gravar o vídeo com a sessão do lab ativa
- [ ] Montar o PDF com links dos 4 repos, do vídeo e das documentações
