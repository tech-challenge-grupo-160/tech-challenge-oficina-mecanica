# Sistema de Gestão de Oficina Mecânica

[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica) [![Cobertura](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica&metric=coverage)](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica&metric=bugs)](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica) [![Vulnerabilidades](https://sonarcloud.io/api/project_badges/measure?project=tech-challenge-grupo-160_tech-challenge-oficina-mecanica&metric=vulnerabilities)](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica)

API REST do **OS Service** da oficina mecânica. O serviço mantém clientes,
veículos e ordens de serviço e orquestra a Saga distribuída com os
microsserviços de Execução e Billing.

## Visão geral

O domínio principal do projeto é a ordem de serviço. A API permite:

- cadastrar e consultar clientes;
- vincular veículos a clientes;
- abrir ordens de serviço com snapshots dos itens consultados na Execução;
- controlar o estado global e o histórico da OS;
- orquestrar diagnóstico, orçamento, reserva, reparo e pagamento por mensagens;
- monitorar tempo de execução e tempo médio de finalização;
- registrar notificações simuladas ao cliente;
- autenticar usuários com JWT.

Catálogo, diagnóstico, estoque e reparo pertencem à Execução. Orçamento e
pagamento pertencem ao Billing. O OS Service não mantém tabelas nem endpoints
locais para essas responsabilidades. Na abertura, ele exige ao menos um serviço,
aceita peças opcionais, consulta `ICatalogoExecucao` por HTTP e persiste somente
os snapshots comerciais necessários à OS.

Ao iniciar o diagnostico, o OS Service responde `202 Accepted`, persiste o estado
da Saga e um comando na Outbox e o publica na fila SQS da Execucao. A Execucao
consome o comando de forma idempotente, cria o diagnostico e publica o evento no
SNS. Somente ao consumir esse evento pela sua fila SQS o OS Service altera a OS
para `EmDiagnostico`. Ao concluir o resultado tecnico, a Execucao publica
`DiagnosticoConcluidoMessage`; o OS Service avanca para `AguardandoOrcamento` e
envia `GerarOrcamentoMessage` para a fila do Billing. Depois que o Billing
publica `OrcamentoGeradoMessage`, o OS Service avança para
`AguardandoAprovacao`. Aprovação, reserva, reparo e pagamento seguem pelo mesmo
modelo assíncrono, com Inbox/Outbox e consumidores idempotentes. O ambiente
Docker inclui LocalStack, filas, filtros de assinatura, tópico e DLQs.

## Arquitetura da Fase 4

O sistema é dividido em três microsserviços, cada um com repositório, banco,
pipeline e deploy próprios ([ADR-0003](docs/adrs/0003-divisao-em-microsservicos.md),
[ADR-0006](docs/adrs/0006-banco-por-servico.md)):

| Serviço | Repositório | Banco | Responsabilidade |
|---|---|---|---|
| **OS Service** | este | PostgreSQL (RDS) | Cliente, veículo, OS, status e histórico; orquestra a Saga |
| **Billing** | [tech-challenge-oficina-mecanica-billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing) | DynamoDB | Orçamento e pagamento no Mercado Pago |
| **Execução** | [tech-challenge-oficina-mecanica-execucao](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao) | PostgreSQL (RDS próprio) | Catálogo, diagnóstico, estoque, reserva de peças e reparo |

Infraestrutura compartilhada: [rede, EKS, API Gateway e ALB](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s),
[bancos RDS](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-database),
[Lambda de autenticação](https://github.com/tech-challenge-grupo-160/tech-challenge-lambda-auth) e
[pipeline reutilizável](https://github.com/tech-challenge-grupo-160/tech-challenge-pipelines).

Os serviços conversam por **SNS + SQS**, uma fila por tipo de mensagem, com
outbox transacional, consumo idempotente e DLQ em toda fila
([ADR-0005](docs/adrs/0005-mensageria-sns-sqs.md),
[catálogo de mensagens](docs/mensageria/catalogo.md)). A única chamada síncrona
entre serviços é a consulta ao catálogo da Execução na abertura da OS.

Diagramas (C4, implantação na AWS e sequência da Saga): [docs/diagrams](docs/diagrams/README.md).

## Saga orquestrada

A OS atravessa os três serviços: diagnóstico na Execução, orçamento no Billing,
aprovação do cliente, reserva de peças e reparo na Execução, pagamento no
Billing e entrega. Não há transação entre bancos diferentes (PostgreSQL e
DynamoDB), então a consistência vem de uma **Saga**: uma sequência de transações
locais, com compensação quando um passo falha.

**Escolhemos a Saga orquestrada, com o orquestrador dentro do OS Service**
([ADR-0004](docs/adrs/0004-saga-orquestrada.md)):

- **O OS Service já era o único dono do status da OS.** Orquestrar é a
  evolução desse papel, sem criar um quarto serviço.
- **O fluxo e as compensações ficam num lugar só.** A máquina de estados
  (`SagaInicioDiagnostico`, persistida no PostgreSQL do OS) mostra em que passo
  cada OS está e o que fazer quando algo falha. Na coreografia, essa lógica
  ficaria espalhada pelos consumidores dos três repositórios.
- **Billing e Execução não se conhecem.** Cada um recebe comandos do
  orquestrador e publica eventos, sem depender dos eventos do outro.
- **É testável e demonstrável.** O fluxo inteiro, com as falhas, está nos
  cenários BDD abaixo.

Descartamos a **coreografia**, em que o fluxo fica implícito e cada serviço
precisa saber quais eventos disparam suas compensações. Descartamos também o
**orquestrador como serviço separado** (mais um repositório e deploy, duplicando
o papel do OS Service) e o **two-phase commit**, inviável entre PostgreSQL e
DynamoDB e bloqueante num fluxo que espera dias pela aprovação do cliente.

Compensações implementadas:

| Falha | Tratamento |
|---|---|
| Orçamento recusado pelo cliente | OS e saga `Cancelada`; nenhuma peça é reservada |
| Falta de estoque na reserva | OS `AguardandoEstoque`; a reposição na Execução retoma a saga e libera o reparo |
| Pagamento recusado | O reparo **não** é desfeito (é o ponto sem volta). A OS segue `Finalizada`, a entrega fica bloqueada e `POST /ordens-servico/{id}/pagamento/nova-tentativa` gera uma nova cobrança sem duplicar pagamento |
| Mensagem repetida ou fora de ordem | Inbox por `MessageId` e validação do estado da saga: o passo não se repete |

Até a execução os passos são compensáveis; a execução é o ponto sem volta; o
pagamento, depois dela, é repetível. Toda mensagem leva o `correlationId` da
saga, que também aparece nos logs dos três serviços.

## Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 16
- MediatR
- JWT Bearer Authentication
- FluentValidation
- xUnit
- Docker e Docker Compose
- Amazon SNS e SQS (LocalStack no desenvolvimento)

## Início rápido

### Com Docker

```bash
docker-compose up --build
```

Endpoints padrão:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`

### Execução local

Acesse a documentação em [docs/SETUP.md](docs/SETUP.md).

## Autenticação

Login:

```http
POST /api/v1/auth/login
```

Seed de desenvolvimento:

- usuário: `admin`
- senha: `admin123`

## Documentação

- [Índice da documentação](docs/README.md)
- [Setup e operação](docs/SETUP.md)
- [Ciclo de vida da infraestrutura](docs/CICLO-DE-VIDA.md)
- [Arquitetura](docs/ARCHITECTURE.md)
- [Infraestrutura](docs/INFRAESTRUTURA.md)
- [Catálogo de eventos e mensageria](docs/mensageria/catalogo.md)
- [Referência da API](docs/API_REFERENCE.md)

## Infraestrutura do OS Service

Os recursos SNS/SQS e a configuração de mensageria do OS Service ficam em
[infra/](infra/README.md), separados da infraestrutura compartilhada de rede e
cluster.

## Infraestrutura na AWS

Os scripts que sobem e derrubam o ambiente ficam neste repositório, porque
orquestram a infraestrutura compartilhada e os seis repositórios — o porquê está em
[docs/CICLO-DE-VIDA.md](docs/CICLO-DE-VIDA.md).

```bash
bash scripts/sobe-tudo.sh
```

```bash
bash scripts/derruba-tudo.sh
```

Primeira vez? Siga o [roteiro rápido](docs/CICLO-DE-VIDA.md#roteiro-rápido):
credencial, Datadog, pipelines, subir, conferir e derrubar, nessa ordem.

Numa conta nova do Learner Lab, ou depois de derrubar o ambiente, exporte antes
a chave do Datadog - ver [Chave do Datadog](docs/CICLO-DE-VIDA.md#chave-do-datadog):

```bash
export TF_VAR_datadog_api_key="<chave da organizacao>"
```

Sem a chave, suba sem o Datadog:

```bash
bash scripts/sobe-tudo.sh --sem-datadog
```

Quem é dono das pipelines renova as credenciais do Learner Lab nos seis
repositórios a cada sessão. Quem não é não roda este comando, porque ele aponta
as pipelines do time para a própria conta:

```bash
bash scripts/renova-secrets.sh
```

> **O cluster cobra sozinho.** O control plane do EKS custa US$ 0,10/hora
> enquanto existir e **não** é suspenso junto com a sessão do lab. Um ambiente
> de pé custa cerca de US$ 5,40/dia. Para ver o que está cobrando agora, sem
> destruir nada:
>
> ```bash
> bash scripts/derruba-tudo.sh --so-conferir
> ```

### Desenvolvimento local

Para rodar só a API, o Docker Compose do [início rápido](#com-docker) resolve.

Para mexer nos manifests do Kubernetes sem cluster na nuvem, existe um ambiente
kind em [`local/`](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s/tree/develop/local),
no `infra-k8s`. **É só desenvolvimento** — nenhum pipeline usa kind desde 04/09,
quando os workflows por runner self-hosted foram aposentados.

O mapa completo de onde cada ambiente roda está em
[docs/INFRAESTRUTURA.md](docs/INFRAESTRUTURA.md).

## Estrutura do repositório

```text
src/
  API/              # Controllers, requests, responses, mappers e bootstrap HTTP
  Application/      # CQRS com Commands, Queries, Handlers, Results e validators
  Domain/           # Entidades, value objects, enums e contratos de repositorio
  Infrastructure/   # EF Core, repositories, migrations, JWT, clock e health checks
  Shared/           # Helpers e templates de logging

tests/
  Fiap.TechChallenge.OficinaMecanica.Test.UnitTests/
  Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests/
docs/
```

## Testes

```bash
dotnet test
```

Os testes de integração e o BDD sobem um PostgreSQL com Testcontainers, então
precisam do Docker rodando.

### Cobertura e qualidade

Cada PR passa pelo SonarCloud, com quality gate de cobertura mínima de 80%. Os
badges no topo deste README mostram a análise da branch principal.

| Serviço | Cobertura (última análise de PR) | Painel |
|---|---|---|
| OS Service | 86,7% | [SonarCloud](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica) |
| Billing | 93,0% | [SonarCloud](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-billing) |
| Execução | 91,5% | [SonarCloud](https://sonarcloud.io/summary/overall?id=tech-challenge-grupo-160_tech-challenge-oficina-mecanica-execucao) |

O job de testes do pipeline também publica a cobertura de linhas no resumo da
execução e o relatório no artefato `coverage-results`.

### BDD do fluxo completo da OS (F4-36)

Os cenários estão em Gherkin, em português, com [Reqnroll](https://reqnroll.net/),
em `tests/Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests/Bdd/Features`:

| Feature | Cenários |
|---|---|
| `FluxoCompletoOS.feature` | da abertura à entrega, passando por Execução e Billing; evento repetido não repete passo da Saga |
| `CompensacoesSaga.feature` | orçamento recusado cancela a OS; falta de estoque segura a OS até a reposição; pagamento recusado não desfaz o reparo e permite nova cobrança; nova cobrança só depois de recusa |

Como os cenários exercitam a Saga:

- A recepção age pela API HTTP do OS Service.
- Billing e Execução entram com os **mesmos eventos que publicam no SNS**,
  entregues aos consumidores reais do OS Service, com inbox, saga, histórico e
  outbox de verdade num PostgreSQL real.
- Os comandos que o OS Service manda para Billing e Execução são conferidos no
  outbox.

```bash
dotnet test tests/Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests --filter "Category=saga"
```

**Relatório de execução:** [`docs/bdd/relatorio-bdd.html`](docs/bdd/relatorio-bdd.html)
guarda o relatório HTML do Reqnroll gerado no CI em 14/11/2026, com os 6
cenários aprovados. O GitHub mostra o HTML como código: baixe o arquivo
(*Download raw file*) e abra no navegador.

A cada PR, o workflow
[Pipeline](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/actions/workflows/pipeline.yml)
roda os cenários de novo. O job `BDD - relatorio` mostra a tabela de cenários no
resumo da execução e publica o relatório atualizado no artefato
`relatorio-bdd`. Para atualizar a cópia em `docs/bdd/`, baixe esse artefato.
Localmente, o relatório sai em
`tests/Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests/bin/<configuracao>/net10.0/TestResults/bdd/relatorio-bdd.html`.

## Pipeline

O workflow [`pipeline.yml`](.github/workflows/pipeline.yml) chama o pipeline
reutilizável [`servico-dotnet.yml@v1`](https://github.com/tech-challenge-grupo-160/tech-challenge-pipelines),
o mesmo dos três serviços:

| Gatilho | O que roda |
|---|---|
| PR para `develop`, `homolog` ou `master` | build, testes unitários, de integração e BDD, SonarCloud e build da imagem (check obrigatório `Pipeline / Build and tests`) |
| push na `homolog` | imagem no ECR (tag = SHA) e deploy no EKS de homologação |
| push na `master` | imagem no ECR e deploy no EKS de produção |
| disparo manual | deploy no ambiente escolhido (é assim que se publica em dev) |

A mensageria do OS (`infra/`) tem workflow próprio, o `terraform-messaging.yml`. Ele roda o plan nos PRs e aplica no push da `develop`.

As branches `homolog` e `master` só recebem alterações por PR, no fluxo
`develop → homolog → master`.

## Documentação da API

- **OpenAPI (Swagger)**: o contrato de cada serviço está versionado no próprio
  repositório: [OS Service](docs/openapi/os-service.json),
  [Billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing/blob/develop/docs/openapi/billing.json),
  [Execução](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao/blob/develop/docs/openapi/execucao.json).
  O Swagger UI de cada serviço fica em `/swagger` no ambiente de
  desenvolvimento. Ele não é exposto no API Gateway.
- **Postman**: em [docs/postman](docs/postman/README.md) estão a collection do
  fluxo completo pelo API Gateway (com obtenção do JWT) e os environments de
  local, dev, hom e prod.
- [Referência da API](docs/API_REFERENCE.md) e [matriz de autorização](docs/MATRIZ_AUTORIZACAO.md).

## Logs e observabilidade

A API escreve um objeto JSON por linha no stdout, pronto para coleta pelo
Datadog Agent. Cada evento contém `timestamp`, `nivel`, `mensagem`, `servico`,
`ambiente`, `rota`, `status`, `duracao` e `traceId`. CPF, JWT, tokens, senhas
e chaves de API são mascarados antes da escrita.

O nível pode ser configurado por ambiente com as seções `Logging:LogLevel` dos
arquivos `appsettings.Development.json` e `appsettings.Production.json`, ou
por variáveis como `Logging__LogLevel__Default`.

## Observações

- Swagger só é exposto em `Development`.
- As rotas de clientes, veículos e ordens de serviço exigem token JWT.
- O `AuthController` administrativo permanece para o fluxo local de funcionários.
- A autenticação externa por CPF e o acesso do cliente continuam sob
  responsabilidade da Lambda/API Gateway.
