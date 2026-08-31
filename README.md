# Sistema de Gestão de Oficina Mecânica

API REST para operação de oficina mecânica com foco em clientes, veículos, catálogo de serviços e peças, autenticação JWT e fluxo completo de ordens de serviço. 

## Visão geral

O domínio principal do projeto é a ordem de serviço. A API permite:

- cadastrar e consultar clientes;
- vincular veículos a clientes;
- manter catálogo de serviços e peças;
- abrir ordens de serviço;
- montar orçamento com serviços e peças;
- controlar o fluxo operacional da OS;
- monitorar tempo de execução e tempo médio de finalização;
- validar estoque antes de iniciar execução;
- gerar e acompanhar pedidos de compra;
- consultar histórico e movimentações de estoque;
- autenticar usuários com JWT.

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
- [Referência da API](docs/API_REFERENCE.md)

## Infraestrutura na AWS

Os scripts que sobem e derrubam o ambiente inteiro ficam neste repositório,
porque orquestram os quatro — o porquê está em
[docs/CICLO-DE-VIDA.md](docs/CICLO-DE-VIDA.md).

```bash
bash scripts/sobe-tudo.sh
```

```bash
bash scripts/derruba-tudo.sh
```

Renovar as credenciais do Learner Lab nos quatro repositórios, a cada sessão:

```bash
bash scripts/renova-secrets.sh
```

> **O cluster cobra sozinho.** O control plane do EKS custa US$ 0,10/hora
> enquanto existir e **não** é suspenso junto com a sessão do lab. Um ambiente
> de pé custa cerca de US$ 5/dia. Para ver o que está cobrando agora, sem
> destruir nada:
>
> ```bash
> bash scripts/derruba-tudo.sh --so-conferir
> ```

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

## Observações

- Swagger só é exposto em `Development`.
- `Clientes`, `Veiculos`, `Servicos`, `Pecas`, `OrdensDeServico` e `PedidosCompra` exigem token JWT.
- `Auth` administrativo permanece na API. O acompanhamento e a resposta da OS sao acessados pelo cliente com JWT emitido pela Lambda.
