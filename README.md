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

```bash
dotnet restore
dotnet run --project src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj
```

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
- [Arquitetura](docs/ARCHITECTURE.md)
- [Referência da API](docs/API_REFERENCE.md)

## Estrutura do repositório

```text
src/
  API/
  Application/
  Domain/
  Infrastructure/
  Shared/

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
- `Auth` e o acompanhamento público de OS estão públicos no estado atual do código.
