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
