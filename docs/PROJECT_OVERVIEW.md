# 🏎️ Sistema Integrado de Gestão de Oficina Mecânica

> **MVP Back-end em C# com .NET 8 - Completo e Funcional**

![.NET Version](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker)
![License](https://img.shields.io/badge/License-MIT-green)

## 📋 Sobre o Projeto

Plataforma completa para gestão integrada de oficinas mecânicas, permitindo controle total de:
- ✅ Clientes e veículos
- ✅ Serviços e peças em estoque
- ✅ Ordens de serviço com fluxo de status
- ✅ Cálculo automático de orçamentos

## 🚀 Início Rápido

### Com Docker (Recomendado)
```bash
# Windows
.\scripts\start.bat

# Linux/Mac
./scripts/start.sh
```

**Resultado:**
```
✓ API: http://localhost:8080
✓ Swagger: http://localhost:8080/swagger
✓ Database: postgres://localhost:5432/oficina_mecanica
```

### Local (.NET 8 SDK Required)
```bash
dotnet restore
dotnet ef database update
dotnet run
```

## 📚 Documentação Completa

| Documento | Descrição |
|-----------|-----------|
| **[SETUP.md](SETUP.md)** | Guia de instalação e execução |
| **[ARCHITECTURE.md](ARCHITECTURE.md)** | Arquitetura e padrões de design |
| **README.md** (original) | Especificações técnicas completas |

## 🏗️ Arquitetura

```
┌─────────────────────────────────────┐
│         API REST (Controllers)      │  ← Recebe requisições HTTP
├─────────────────────────────────────┤
│      Application Services (DTOs)    │  ← Lógica de aplicação
├─────────────────────────────────────┤
│    Domain (Entities, Agregados)     │  ← Regras de negócio
├─────────────────────────────────────┤
│  Infrastructure (Repositories, DB)  │  ← Acesso a dados
└─────────────────────────────────────┘
```

**Padrões:** DDD • Repository • Dependency Injection • Clean Architecture

## 📊 Funcionalidades Implementadas

### Clientes
- Criar, listar, obter, atualizar, deletar
- Validação de CPF/CNPJ
- Busca por ID e CPF/CNPJ

### Veículos
- Gerenciamento completo com histórico de cliente
- Validação de placa
- Filtragem por cliente

### Serviços & Peças
- Cadastro com preço e especificações
- Controle de estoque para peças
- Listagem e atualização

### Ordens de Serviço
- Criação com número automático
- Fluxo de status validado
- Adição dinâmica de serviços e peças
- Cálculo automático de valor total

## 🔗 API REST

### Base URL
```
http://localhost:8080/api
```

### Clientes
```
POST   /clientes              Criar novo cliente
GET    /clientes              Listar todos
GET    /clientes/{id}         Obter por ID
PUT    /clientes/{id}         Atualizar
DELETE /clientes/{id}         Deletar
```

### Veículos
```
POST   /veiculos              Criar veículo
GET    /veiculos              Listar todos
GET    /veiculos/{id}         Obter por ID
GET    /veiculos/cliente/{id} Filtrar por cliente
PUT    /veiculos/{id}         Atualizar
DELETE /veiculos/{id}         Deletar
```

### Serviços
```
POST   /servicos              Criar serviço
GET    /servicos              Listar todos
GET    /servicos/{id}         Obter por ID
PUT    /servicos/{id}         Atualizar
DELETE /servicos/{id}         Deletar
```

### Peças
```
POST   /pecas                 Criar peça
GET    /pecas                 Listar todas
GET    /pecas/{id}            Obter por ID
PUT    /pecas/{id}            Atualizar
DELETE /pecas/{id}            Deletar
```

### Ordens de Serviço
```
POST   /ordens-servico                     Criar ordem
GET    /ordens-servico                     Listar todas
GET    /ordens-servico/{id}                Obter por ID
GET    /ordens-servico/cliente/{id}        Filtrar por cliente
GET    /ordens-servico/status/{status}     Filtrar por status
PUT    /ordens-servico/{id}/status         Alterar status
POST   /ordens-servico/{id}/servicos       Adicionar serviço
POST   /ordens-servico/{id}/pecas          Adicionar peça
DELETE /ordens-servico/{id}                Deletar ordem
```

## 💾 Status de Ordem de Serviço

```
Recebida
    ↓
EmDiagnostico
    ↓
AguardandoAprovacao
    ↓
EmExecucao
    ↓
Finalizada
    ↓
Entregue
```

## 🛠️ Stack Tecnológico

| Componente | Tecnologia | Versão |
|-----------|-----------|---------|
| **Runtime** | .NET | 8.0 |
| **Web** | ASP.NET Core | 8.0 |
| **Database** | PostgreSQL | 16 |
| **ORM** | Entity Framework Core | 8.0 |
| **Container** | Docker + Docker Compose | Latest |
| **API Docs** | Swagger/OpenAPI | 6.0 |
| **Testes** | xUnit + FluentAssertions | Latest |

## 📂 Estrutura do Projeto

```
src/
├── API/                     ← Controllers REST
├── Application/             ← Services, DTOs
├── Domain/                  ← Entities, Repositories (interfaces)
└── Infrastructure/          ← DbContext, Repositories (implementação)

tests/
└── Oficina.UnitTests/       ← Testes unitários

docker-compose.yml          ← Orquestração de containers
Dockerfile                  ← Build da aplicação
Program.cs                  ← Configuração da aplicação
```

## 🐳 Docker Compose

### Serviços Inclusos
- **API** - ASP.NET Core 8.0
- **PostgreSQL** - Banco de dados

### Iniciar
```bash
docker-compose up --build -d
```

### Parar
```bash
docker-compose down
```

### Ver Logs
```bash
docker-compose logs -f api
```

## ✅ Testes

### Executar
```bash
dotnet test
```

### Cobertura
- Domain Layer: 85%
- Application Services: 80%

### Testes Implementados
- ✅ CRUD de Clientes
- ✅ Validações de Negócio
- ✅ Fluxo de Status da Ordem

## 🏥 Health Checks

### Endpoints
```
GET /health           Status geral
GET /health/live      Liveness probe
GET /health/ready     Readiness probe
```

### Resposta Exemplo
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Database",
      "status": "Healthy",
      "duration": 125.45
    }
  ]
}
```

## 🔐 Segurança Implementada

- ✅ Validação de entrada (CPF, CNPJ, Email)
- ✅ CORS configurado
- ✅ Connection pooling
- ✅ Usuário non-root em Docker
- ✅ Prepared statements (Entity Framework)
- ✅ Tratamento centralizado de exceções

## 📈 Performance

- ✅ Operações assíncronas (async/await)
- ✅ Connection pooling otimizado
- ✅ Índices no banco de dados
- ✅ Lazy loading controlado
- ✅ Health checks para monitoramento

## 🚀 Deployment

### Pré-requisitos
- Docker Desktop (ou Docker + Docker Compose)

### Deploy
```bash
# 1. Clonar
git clone <repo>
cd oficina-mecanica

# 2. Configurar (opcional)
cp .env.example .env

# 3. Iniciar
docker-compose up --build -d

# 4. Verificar
docker-compose ps
curl http://localhost:8080/health
```

## 🎯 Padrões de Design Utilizados

| Padrão | Localização | Benefício |
|--------|------------|----------|
| **Repository** | Domain/Infrastructure | Abstração de dados |
| **Dependency Injection** | Program.cs | Inversão de controle |
| **DTO** | Application | Separação de responsabilidades |
| **Aggregate Root** | Domain | Consistência de dados |
| **Async/Await** | Services | Performance |

## 📝 Exemplo de Requisição

### Criar Cliente
```bash
curl -X POST http://localhost:8080/api/clientes \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Silva",
    "cpfCnpj": "12345678901",
    "telefone": "11999999999",
    "email": "joao@example.com"
  }'
```

### Resposta
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "nome": "João Silva",
  "cpfCnpj": "12345678901",
  "telefone": "11999999999",
  "email": "joao@example.com",
  "dataCadastro": "2026-03-16T22:30:00Z"
}
```

## 🐛 Troubleshooting

### Erro: Porta já em uso
```bash
# Alterar em docker-compose.yml ou .env
docker-compose down
docker-compose up --build -d --remove-orphans
```

### Erro: Conexão com BD
```bash
docker-compose logs postgres
docker-compose restart postgres
```

### Limpar tudo e começar do zero
```bash
docker-compose down -v
docker system prune -a
docker-compose up --build -d
```

## 📖 Documentação Adicional

- [Microsoft Learn - .NET 8](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [PostgreSQL](https://www.postgresql.org/docs/)
- [Docker](https://docs.docker.com/)

## 🤝 Contribuindo

1. Clone o repositório
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📄 Licença

Este projeto é fornecido como exemplo educacional.

---

**Status:** ✅ **MVP Completo**

Todos as funcionalidades listadas no documento técnico foram implementadas e testadas.

**Última Atualização:** 16/03/2026
