# Setup e Operação

## Pré-requisitos

### Execução com Docker

- Docker Desktop
- Docker Compose

### Execução local

- .NET SDK 10
- PostgreSQL 16 ou superior

## Configuração por variáveis de ambiente

O projeto usa `.env` no fluxo com Docker. Use `.env.example` como base.

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=oficina_mecanica
DB_PORT=5432
API_PORT=8080
API_CONNECTION_STRING=Host=postgres;Database=oficina_mecanica;Username=postgres;Password=postgres
JWT_SECRET=defina-uma-chave-muito-forte
JWT_ISSUER=Fiap.TechChallenge.OficinaMecanica
JWT_AUDIENCE=Fiap.TechChallenge.OficinaMecanica
```

## Execução com Docker

```bash
docker-compose up --build
```

Ao iniciar em `Development`, a aplicação:

- aguarda o PostgreSQL ficar saudável;
- executa migrations automaticamente;
- executa seed de dados de desenvolvimento;
- expõe Swagger em `/swagger`.

### Endpoints padrão

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
- PostgreSQL: `localhost:5432`

### Credenciais iniciais

O seed cria um usuário para autenticação:

- usuário: `admin`
- senha: `admin123`

## Execução local

### 1. Criar a base

```sql
CREATE DATABASE oficina_mecanica;
```

### 2. Configurar secrets

Na raiz do projeto:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=oficina_mecanica;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:SecretKey" "defina-uma-chave-muito-forte"
dotnet user-secrets set "Jwt:Issuer" "Fiap.TechChallenge.OficinaMecanica"
dotnet user-secrets set "Jwt:Audience" "Fiap.TechChallenge.OficinaMecanica"
```

### 3. Restaurar e executar

```bash
dotnet restore
dotnet run --project src/API/Fiap.TechChallenge.OficinaMecanica.Api.csproj
```

Observações:

- fora do Docker, a aplicação também executa `MigrateAndSeedAsync` quando o ambiente estiver em `Development`;
- o Swagger é habilitado apenas nesse ambiente.

## Testes

```bash
dotnet test
```

Para rodar suites separadas:

```bash
dotnet test tests/Fiap.TechChallenge.OficinaMecanica.Test.UnitTests/Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.csproj
dotnet test tests/Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests/Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.csproj
```

## Operação básica

### Subir em background

```bash
docker-compose up --build -d
```

### Parar serviços

```bash
docker-compose down
```

### Remover volumes e reinicializar o banco

```bash
docker-compose down -v
docker-compose up --build
```

### Ver logs

```bash
docker-compose logs -f api
docker-compose logs -f postgres
```

## Verificações rápidas

### Health check

```bash
curl http://localhost:8080/health
```

### Login

```bash
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "admin",
    "senha": "admin123"
  }'
```

## Troubleshooting

### Porta ocupada

Se `5432` ou `8080` já estiverem em uso, ajuste `DB_PORT` e `API_PORT` no `.env`.

### Banco inconsistente após mudanças locais

```bash
docker-compose down -v
docker-compose up --build
```

### API não sobe por falha de conexão

Verifique:

- se o container `postgres` está saudável;
- se a connection string está correta;
- se `JWT_SECRET` foi definido;
- se o banco aceita conexões na porta configurada.

### Sem Swagger

O Swagger só é exposto quando `ASPNETCORE_ENVIRONMENT=Development`.
