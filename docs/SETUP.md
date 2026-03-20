# Setup e Execução - Sistema Integrado de Gestão de Oficina Mecânica

## Pré-requisitos

### Opção 1: Com Docker (Recomendado)
- Docker Desktop instalado ([Download](https://www.docker.com/products/docker-desktop))
- Docker Compose incluído no Docker Desktop

### Opção 2: Local
- .NET 8 SDK ([Download](https://dotnet.microsoft.com/download))
- PostgreSQL 16+ ([Download](https://www.postgresql.org/download/))

---

## Execução com Docker (Recomendado)

### Windows

```powershell
cd C:\Users\Lucas\Repos\oficina-mecanica\
.\scripts\start.bat
```

### Linux/Mac

```bash
cd /path/to/oficina-mecanica/
chmod +x scripts/start.sh
./scripts/start.sh
```

### Usando Docker Compose diretamente

```bash
docker-compose up --build -d
```

**Resultado esperado:**
- API disponível em `http://localhost:8080`
- Swagger UI em `http://localhost:8080/swagger`
- PostgreSQL em `localhost:5432`

---

## Execução Local (Sem Docker)

### 1. Criar banco de dados PostgreSQL

```sql
CREATE DATABASE oficina_mecanica;
```

### 2. Configurar connection string

Editar `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=oficina_mecanica;Username=postgres;Password=sua_senha"
  }
}
```

### 3. Restaurar dependências

```bash
dotnet restore
```

### 4. Aplicar migrations

```bash
dotnet ef database update
```

### 5. Executar a aplicação

```bash
dotnet run
```

A API estará disponível em `https://localhost:7063` ou `http://localhost:5000`

---

## Parar os Containers

### Docker Compose

```bash
# Parar containers mantendo volumes
docker-compose down

# Parar containers e remover volumes
docker-compose down -v
```

---

## Visualizar Logs

```bash
# Logs da API
docker-compose logs -f api

# Logs do PostgreSQL
docker-compose logs -f postgres

# Todos os logs
docker-compose logs -f
```

---

## Endpoints da API

### Base URL
```
http://localhost:8080/api
```

### Clientes
- `POST /clientes` - Criar cliente
- `GET /clientes` - Listar clientes
- `GET /clientes/{id}` - Obter cliente
- `PUT /clientes/{id}` - Atualizar cliente
- `DELETE /clientes/{id}` - Deletar cliente

### Veículos
- `POST /veiculos` - Criar veículo
- `GET /veiculos` - Listar veículos
- `GET /veiculos/{id}` - Obter veículo
- `GET /veiculos/cliente/{clienteId}` - Listar por cliente
- `PUT /veiculos/{id}` - Atualizar veículo
- `DELETE /veiculos/{id}` - Deletar veículo

### Serviços
- `POST /servicos` - Criar serviço
- `GET /servicos` - Listar serviços
- `GET /servicos/{id}` - Obter serviço
- `PUT /servicos/{id}` - Atualizar serviço
- `DELETE /servicos/{id}` - Deletar serviço

### Peças
- `POST /pecas` - Criar peça
- `GET /pecas` - Listar peças
- `GET /pecas/{id}` - Obter peça
- `PUT /pecas/{id}` - Atualizar peça
- `DELETE /pecas/{id}` - Deletar peça

### Ordens de Serviço
- `POST /ordens-servico` - Criar ordem
- `GET /ordens-servico` - Listar ordens
- `GET /ordens-servico/{id}` - Obter ordem
- `GET /ordens-servico/cliente/{clienteId}` - Listar por cliente
- `GET /ordens-servico/status/{status}` - Listar por status
- `PUT /ordens-servico/{id}/status` - Atualizar status
- `POST /ordens-servico/{id}/servicos` - Adicionar serviço
- `POST /ordens-servico/{id}/pecas` - Adicionar peça
- `DELETE /ordens-servico/{id}` - Deletar ordem

---

## Documentação Swagger

Acesse a documentação interativa em:
```
http://localhost:8080/swagger
```

Aqui você pode:
- Visualizar todos os endpoints
- Ver modelos de requisição/resposta
- Testar os endpoints diretamente

---

## Troubleshooting

### Porta 5432 já está em uso
```bash
# Encontrar container usando a porta
docker ps

# Remover container
docker rm -f <container_id>

# Ou alterar a porta no docker-compose.yml
# Mudar "5432:5432" para "5433:5432"
```

### Porta 8080 já está em uso
```bash
# Alterar no docker-compose.yml
# Mudar "8080:8080" para "8081:8080"
```

### Erro de conexão com banco de dados
```bash
# Verificar se PostgreSQL está rodando
docker-compose ps

# Reiniciar containers
docker-compose restart

# Ou executar novamente
docker-compose down
docker-compose up --build -d
```

### Clear de migrations locais
```bash
# Remover migrations e banco
dotnet ef database drop -f
dotnet ef database update
```

---

## Variáveis de Ambiente

### Docker Compose
Definidas em `docker-compose.yml`:
- `ASPNETCORE_ENVIRONMENT` = Production
- `ConnectionStrings__DefaultConnection` = Connection string do PostgreSQL

### Local Development
Defina em `appsettings.Development.json` ou variáveis de ambiente do SO

---

## Estrutura do Projeto

```
oficina-mecanica/
├── src/
│   ├── API/
│   │   └── Controllers/
│   ├── Application/
│   │   ├── DTOs/
│   │   └── Services/
│   ├── Domain/
│   │   ├── Entities/
│   │   └── Repositories/
│   └── Infrastructure/
│       ├── Data/
│       └── Repositories/
├── tests/
│   └── Oficina.UnitTests/
├── Dockerfile
├── docker-compose.yml
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

---

## Desenvolvimento

### Rodar testes
```bash
dotnet test
```

### Criar nova migration
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Build local
```bash
dotnet build
```

---

## Suporte

Para mais informações, consulte:
- [Documentação .NET 8](https://learn.microsoft.com/en-us/dotnet/)
- [Docker Documentation](https://docs.docker.com/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/)
