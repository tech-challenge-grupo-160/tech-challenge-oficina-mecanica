# Sistema de Gestão de Oficina Mecânica

API REST para gestão de clientes, veículos, serviços, peças e ordens de serviço de uma oficina mecânica. O projeto segue uma arquitetura em camadas, utiliza ASP.NET Core, Entity Framework Core, PostgreSQL e autenticação JWT.

## Visão geral

O domínio principal do sistema é a ordem de serviço. A aplicação permite:

- cadastrar e consultar clientes;
- associar veículos a clientes;
- manter catálogo de serviços e peças;
- abrir ordens de serviço;
- adicionar serviços e peças à ordem;
- controlar o fluxo de status da manutenção;
- expor tudo por meio de uma API HTTP documentada via Swagger.

## Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 16
- JWT Bearer Authentication
- FluentValidation
- xUnit para testes
- Docker e Docker Compose

## Início rápido

### Com Docker

1. Copie `.env.example` para `.env`.
2. Ajuste as variáveis de ambiente, principalmente `JWT_SECRET`.
3. Suba os serviços:

```bash
docker-compose up --build
```

Endpoints principais:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health check: `http://localhost:8080/health`

### Execução local

1. Instale o .NET SDK 10 e PostgreSQL.
2. Configure a connection string e o segredo JWT via `dotnet user-secrets` ou variáveis de ambiente.
3. Execute:

```bash
dotnet restore
dotnet run --project Fiap.TechChallenge.OficinaMecanica.Api.csproj
```

## Autenticação

O login está disponível em `POST /api/v1/auth/login`.

Ambientes de desenvolvimento iniciados com Docker executam migration e seed automaticamente, incluindo um usuário inicial:

- usuário: `admin`
- senha: `admin123`

## Documentação

- [Índice da documentação](docs/README.md)
- [Setup e operação local](docs/SETUP.md)
- [Arquitetura](docs/ARCHITECTURE.md)
- [Referência da API](docs/API_REFERENCE.md)

## Estrutura do repositório

```text
src/
  API/             Controllers e filtros HTTP
  Application/     Serviços de aplicação, DTOs e validators
  Domain/          Entidades e contratos de repositório
  Infrastructure/  EF Core, repositórios, migrations, seed e health checks

Fiap.TechChallenge.OficinaMecanica.Test.UnitTests/
Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests/
docker/
docs/
```

## Testes

```bash
dotnet test
```

## Observações

- O Swagger é habilitado em ambiente `Development`.
- `Clientes`, `Veiculos` e `Pecas` exigem token JWT.
- `Servicos`, `OrdensDeServico` e `Auth` estão expostos sem `[Authorize]` no estado atual do código.

```csharp
public class Servico
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}
```

---

### Peca

Representa peças e insumos utilizados nos serviços.

```csharp
public class Peca
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}
```

---

# 5. Fluxo de Ordem de Serviço

## Criação da OS

Fluxo principal:

1. Identificação do cliente por CPF ou CNPJ
2. Cadastro ou seleção do veículo
3. Inclusão dos serviços solicitados
4. Inclusão de peças e insumos necessários
5. Cálculo automático do orçamento
6. Envio do orçamento para aprovação do cliente

---

## Status da Ordem de Serviço

Estados possíveis:

```
Recebida
EmDiagnostico
AguardandoAprovacao
EmExecucao
Finalizada
Entregue
```

### Fluxo de estados

```
Recebida → EmDiagnostico
EmDiagnostico → AguardandoAprovacao
AguardandoAprovacao → EmExecucao
EmExecucao → Finalizada
Finalizada → Entregue
```

---

# 6. APIs REST

## Clientes

```
POST   /api/clientes
GET    /api/clientes
GET    /api/clientes/{id}
PUT    /api/clientes/{id}
DELETE /api/clientes/{id}
```

---

## Veículos

```
POST   /api/veiculos
GET    /api/veiculos
GET    /api/veiculos/{id}
```

---

## Ordens de Serviço

```
POST   /api/ordens-servico
GET    /api/ordens-servico
GET    /api/ordens-servico/{id}
PUT    /api/ordens-servico/{id}/status
```

---

## Peças

```
POST   /api/pecas
GET    /api/pecas
PUT    /api/pecas/{id}
```

---

# 7. Segurança

## Autenticação

As APIs administrativas são protegidas utilizando **JWT (JSON Web Token)**.

### Fluxo de autenticação

1. Usuário realiza login
2. API gera um token JWT
3. O token é enviado nas requisições

```
Authorization: Bearer {token}
```

---

## Validação de Dados Sensíveis

### CPF / CNPJ

* Validação de formato
* Verificação de dígitos verificadores

### Placa de Veículo

Regex para padrão Mercosul:

```
[A-Z]{3}[0-9][A-Z][0-9]{2}
```

---

# 8. Banco de Dados

Banco escolhido: **PostgreSQL**

### Justificativa

* Open Source
* Alta confiabilidade
* Excelente suporte no .NET
* Forte suporte a transações

### ORM utilizado

* Entity Framework Core

---

# 9. Testes Automatizados

## Testes Unitários

Ferramentas:

* xUnit
* FluentAssertions

Cobertura em:

* Domínio
* Regras de negócio
* Application Services

---

## Testes de Integração

Validam:

* APIs
* Repositórios
* Integração com banco

Ferramentas:

* WebApplicationFactory
* TestContainers

### Meta de cobertura

**80% de cobertura nos domínios críticos**

---

# 10. Documentação da API

A documentação será gerada automaticamente utilizando **Swagger / OpenAPI**.

Endpoint:

```
/swagger
```

Permite:

* Testar endpoints
* Visualizar contratos da API
* Validar requisições

---

# 11. Containerização

## Dockerfile

Responsável por construir a imagem da aplicação.

### Etapas

1. Build da aplicação
2. Publicação
3. Execução dentro do container

---

## Docker Compose

Utilizado para orquestrar o ambiente.

### Serviços

```
api
postgres
```

### Inicialização

```
docker-compose up --build
```

---

# 12. Estrutura do Projeto

```
src/

Oficina.API
Oficina.Application
Oficina.Domain
Oficina.Infrastructure

tests/

Oficina.UnitTests
Oficina.IntegrationTests
```

---

# 13. Execução do Projeto

## Pré-requisitos

* Docker
* .NET 10 SDK

---

## Execução com Docker

```
docker-compose up --build
```

---

## Execução local

```
dotnet restore
dotnet build
dotnet run
```

---

# 14. Análise de Vulnerabilidades

Ferramentas recomendadas:

* Snyk
* OWASP Dependency Check
* SonarQube

### Itens analisados

* Dependências vulneráveis
* Segurança de autenticação
* Validação de dados
* Exposição de endpoints

---

# 15. Conclusão

A solução proposta atende aos requisitos do desafio ao fornecer:

* Arquitetura baseada em DDD
* Back-end monolítico em camadas
* APIs REST documentadas
* Autenticação segura com JWT
* Testes automatizados
* Ambiente containerizado com Docker

Este MVP estabelece uma base sólida para evolução futura do sistema, incluindo integrações com aplicativos móveis e dashboards administrativos.
