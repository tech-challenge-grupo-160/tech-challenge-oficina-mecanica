# Docker Setup Guide - Operação Prática

## Pré-requisitos
- Docker Desktop
- .NET 10 SDK
- Git

## Iniciar

```bash
docker-compose down -v         # Limpar volumes antigos
docker-compose up --build      # Reconstruir e iniciar
```

Aguarde 40-60 segundos pelos logs de sucesso.

## Parar

```bash
docker-compose stop            # Apenas parar (manter dados)
docker-compose down            # Parar e remover (manter dados)
docker-compose down -v         # Parar, remover e deletar dados
```

## Acessar

- **Swagger UI**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/health
- **API Clientes**: http://localhost:8080/api/v1/clientes
- **PostgreSQL**: localhost:5432 (user: postgres, pass: postgres)

## Logs

```bash
# API logs
docker logs -f oficina-mecanica-api-dev | grep -i "migration\|seed"

# PostgreSQL logs
docker logs -f oficina-mecanica-postgres-dev

# Todos os logs
docker logs oficina-mecanica-api-dev
docker logs oficina-mecanica-postgres-dev
```

## Verificar Banco

### Com psql (CLI PostgreSQL)
```bash
psql -h localhost -U postgres -d oficina_mecanica

# Listar tabelas
\dt

# Contar clientes
SELECT COUNT(*) FROM "Cliente";
```

### Com Docker exec
```bash
docker exec oficina-mecanica-postgres-dev psql -U postgres -d oficina_mecanica \
  -c "SELECT COUNT(*) FROM \"Cliente\";"
```

### Com DBeaver (GUI)
1. Nova conexão PostgreSQL
2. Host: localhost
3. Port: 5432
4. Database: oficina_mecanica
5. User: postgres
6. Password: postgres

## Testar API

```bash
# Health
curl http://localhost:8080/health

# Clientes
curl http://localhost:8080/api/v1/clientes

# Veículos
curl http://localhost:8080/api/v1/veiculos

# Serviços
curl http://localhost:8080/api/v1/servicos
```

## Troubleshooting

### "relation X does not exist"
```bash
docker-compose down -v
docker-compose up --build
```

### PostgreSQL não conecta
```bash
# Verificar status
docker ps | grep postgres

# Ver logs
docker logs oficina-mecanica-postgres-dev

# Testar healthcheck
docker exec oficina-mecanica-postgres-dev pg_isready -U postgres
```

### API não conecta ao Postgres
```bash
# Verificar string de conexão
docker exec oficina-mecanica-api-dev env | grep ConnectionString

# Ver logs de erro
docker logs oficina-mecanica-api-dev | grep -i "error\|exception"
```

### Dados não aparecem
```bash
# Verificar environment
docker exec oficina-mecanica-api-dev env | grep ASPNETCORE_ENVIRONMENT

# Deve estar como "Development" para executar seeding
# Se em Production, mudar em docker-compose.yml
```

### Migrations não executam
```bash
# Verificar migrations foram aplicadas
docker exec oficina-mecanica-postgres-dev psql -U postgres -d oficina_mecanica \
  -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

## Modificar Dados de Seed

1. Edite `src/Infrastructure/Data/Seeders/OficinaDbContextSeeder.cs`
2. Adicione novo cliente, veículo, etc
3. Recrie containers:
```bash
docker-compose down -v
docker-compose up --build
```

## Ambiente Production

Para desabilitar seeding em production:
```yaml
# docker-compose.yml
environment:
  ASPNETCORE_ENVIRONMENT: Production  # Muda de Development
```

Seeding **só executa** quando `ASPNETCORE_ENVIRONMENT=Development`


