# ADR-0001: Segregar o monorepo em quatro repositórios

| | |
|---|---|
| **Status** | Proposta |
| **Data** | 2026-08-11 |
| **Decisores** | Grupo 160 |
| **Issue** | [#47](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/47) |

## Contexto

Até a Fase 2 o projeto viveu em um único repositório: API .NET, Terraform do cluster, manifests Kubernetes, Dockerfiles e documentação juntos. Isso funcionou enquanto tudo era implantado no mesmo lugar — um cluster `kind` local, por um runner self-hosted.

A Fase 3 exige explicitamente **quatro repositórios separados**, cada um com CI/CD próprio, branch principal protegida e deploy automático para homologação e produção.

Além da exigência formal, há razões técnicas que já apareciam no monorepo:

- **Ciclos de vida diferentes.** A infraestrutura muda raramente e com alto risco; a aplicação muda toda semana. Hoje qualquer commit dispara o mesmo pipeline.
- **Permissões iguais para riscos diferentes.** Quem altera um validador da API tem o mesmo acesso de quem altera o Terraform que destrói um cluster.
- **Pipelines acoplados.** O workflow atual constrói a solução .NET *e* aplica Terraform. Uma falha de teste unitário bloqueia uma correção de infraestrutura que nada tem a ver.

## Decisão

Dividir em quatro repositórios, na organização `tech-challenge-grupo-160`:

| Repositório | Conteúdo | Origem |
|---|---|---|
| `tech-challenge-oficina-mecanica` | API .NET, `docker/`, testes, documentação arquitetural | Repositório atual, mantido |
| `tech-challenge-lambda-auth` | Function serverless de autenticação por CPF | Novo |
| `tech-challenge-infra-k8s` | Terraform da rede e do cluster, manifests `k8s/` | Extraído de `infra/` e `k8s/` |
| `tech-challenge-infra-database` | Terraform do banco gerenciado | Novo |

### Preservação de histórico

O `tech-challenge-infra-k8s` foi criado **com o histórico git preservado** dos caminhos `infra/` e `k8s/`, via `git filter-branch` com `index-filter` sobre um clone bare. Dos 260 commits do monorepo, 41 tocaram esses diretórios e foram mantidos, a partir de `feat: inclusao dos arquivos de config k8s` (2026-06-16).

Os outros dois repositórios nascem sem histórico porque seu conteúdo não existia antes.

### Visibilidade

Os quatro repositórios são **públicos**. Essa não foi uma escolha de preferência: a organização está no plano **GitHub Free**, onde branch protection só está disponível em repositórios públicos. Como a Fase 3 exige branch principal protegida sem commits diretos, repositório privado tornaria o requisito inviável sem upgrade de plano.

Consequência direta: **nenhum segredo pode ser commitado em nenhum dos quatro repositórios**. Credenciais vão para o gerenciador de segredos da nuvem, e os pipelines autenticam por OIDC.

### Convenção de branches

Os repositórios novos usam `main`. O repositório principal mantém `master` por já ter automações e histórico apontando para ela — renomear traria retrabalho sem benefício proporcional dentro do prazo da fase.

Fluxo mantido em todos: `feature/*` → `develop` → `homolog` → `release/*` → branch principal.

## Alternativas consideradas

### Manter o monorepo

Descartada por conflito direto com o requisito da Fase 3, que é avaliado. Tecnicamente seria possível ter pipelines separados por path filter, mas isso não atende ao enunciado.

### Dividir sem preservar histórico

Mais simples e rápido: copiar os arquivos e fazer um commit inicial. Descartada porque o histórico da infraestrutura mostra a evolução real do trabalho — inclusive as correções de kubeconfig no runner — e isso tem valor tanto para a avaliação quanto para entender decisões passadas.

### Um repositório único de infraestrutura

Juntar cluster e banco em `tech-challenge-infra`. Reduziria a duplicação de pipeline e de configuração de state. Descartada porque o enunciado lista explicitamente "Infraestrutura Kubernetes" e "Infraestrutura do Banco de Dados Gerenciado" como repositórios distintos.

## Consequências

### Positivas

- Pipelines independentes: uma falha de teste da API não bloqueia mudança de infraestrutura
- Permissões e revisores podem ser diferentes por repositório, ajustados ao risco
- O `terraform plan` de cada repositório fica legível, sem ruído da aplicação
- Histórico da infraestrutura preservado e agora isolado do ruído da API

### Negativas

- **Mudança que atravessa repositórios exige múltiplos PRs coordenados.** Alterar uma variável de ambiente da API pode exigir PR no repositório da aplicação e no de infraestrutura, sem atomicidade entre eles.
- **Quatro pipelines para manter**, com configuração de OIDC, secrets e branch protection replicada.
- **Rastreabilidade fica mais difícil.** Descobrir qual commit de infraestrutura acompanhou qual versão da API passa a depender de disciplina de tags e de referências cruzadas nos PRs.
- **Duplicação de configuração**: templates de issue, `.gitignore` e workflows repetidos em quatro lugares, sujeitos a divergir com o tempo.

### Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Segredo commitado em repositório público | `.gitignore` cobrindo `.env`, `*.pem`, `*.key` e `*.tfvars.local`; credenciais só no gerenciador de segredos; revisão obrigatória em PR |
| Deriva entre os quatro repositórios | Referência cruzada obrigatória nos READMEs; issue de checagem no fim da fase |
| Estado do Terraform corrompido por execução concorrente | Backend remoto com lock ([#57](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/57)) |
| Deploy da API contra infraestrutura incompatível | `apply` da infraestrutura roda antes do deploy da aplicação; migrations aplicadas pelo pipeline antes da nova versão ([#79](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/79)) |

## Situação na data desta ADR

Os quatro repositórios já existem e estão com branch principal protegida (1 aprovação obrigatória, `enforce_admins` ativo, force push e deleção bloqueados). O que falta é a autenticação OIDC com a nuvem ([#54](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/54)), sem a qual os pipelines só executam a parte de integração contínua — a etapa de deploy fica sem destino.

> Esta ADR foi escrita **após** a execução, documentando decisões já tomadas. O grupo deve revisá-la e, discordando de qualquer ponto, registrar a mudança aqui antes que mais trabalho se acumule sobre a estrutura atual.

## Referências

- [Épico E2 — Segregação em 4 Repositórios e CI/CD](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/29)
- [GitHub — About protected branches](https://docs.github.com/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
