# Docker + Migrations + Seeding - Documentação Técnica

## Visão Geral

Setup automático de migrations e seeding quando os containers Docker iniciam.

## 🏗️ Componentes

### 1. Docker Compose Health Checks
- PostgreSQL com `pg_isready` (10s intervalo, 5 tentativas)
- API aguarda PostgreSQL estar saudável

### 2. HostExtensions - `src/Infrastructure/Extensions/HostExtensions.cs`
```csharp
await app.MigrateAndSeedAsync(app.Environment.IsDevelopment());
```

Funcionalidades:
- Retry automático: 6 tentativas × 5s = 30s máximo
- Aguarda disponibilidade do PostgreSQL
- Executa migrations com `context.Database.MigrateAsync()`
- Seeding condicional (apenas em Development)
- Logs detalhados

### 3. Seeder - `src/Infrastructure/Data/Seeders/OficinaDbContextSeeder.cs`
- Verifica `.AnyAsync()` antes de inserir
- Insere 21 registros de teste
- Sem duplicatas em múltiplas execuções

### 4. DbContext Mapping - `src/Infrastructure/Data/OficinaDbContext.cs`
```csharp
entity.ToTable("Cliente");                    // Nome da tabela
entity.Property(e => e.TempoEstimado)
    .HasColumnName("TempoEstimadoMinutos");   // Mapeamento coluna
```

## 🚀 Iniciar

```bash
docker-compose down -v      # Limpar dados
docker-compose up --build   # Reconstruir e iniciar
```

## Logs Esperados

```
Iniciando processo de migration e seeding do banco de dados...
Tentativa 1/6 de executar migrations...
Migrations executadas com sucesso!
Iniciando seeding de dados mocados...
Seeding completado com sucesso!
```

## 📊 Dados Inseridos

| Entidade | Qty | Exemplos |
|----------|-----|----------|
| Clientes | 4 | Vanessa Luna Duarte, Rafael Mateus Cesar Souza, Betina, Vicente |
| Veículos | 3 | Toyota Corolla, Honda Civic, VW Gol |
| Serviços | 5 | Troca Óleo, Revisão, Alinhamento, Pneus, Diagnóstico |
| Peças | 5 | Filtros, pastilhas, pneus, velas |
| Ordens | 2 | OS-001, OS-002 |
| Relacionamentos | 6 | Ordem x Serviço (3) + Ordem x Peça (3) |
| **TOTAL** | **21** | |

## ⚙️ Configuração

### Variáveis de Ambiente
```yaml
ASPNETCORE_ENVIRONMENT: Development  # Ativa seeding
ConnectionStrings__DefaultConnection: "Host=postgres;Database=oficina_mecanica;..."
```

### Retry (HostExtensions)
```csharp
maxRetries: 6              // 6 tentativas
delayMilliseconds: 5000    // 5s entre tentativas
// Total: até 30s de espera
```

## 🔍 Verificação

### Health Check
```bash
curl http://localhost:8080/health
```

### Contar Clientes
```bash
docker exec oficina-mecanica-postgres-dev psql -U postgres -d oficina_mecanica \
  -c "SELECT COUNT(*) FROM \"Cliente\";"
# Esperado: 3
```

### Ver Logs
```bash
docker logs oficina-mecanica-api-dev | grep -i "migration\|seed"
```

## 🐛 Troubleshooting

| Erro | Solução |
|------|---------|
| "relation X does not exist" | `docker-compose down -v && docker-compose up --build` |
| PostgreSQL não responde | Aumentar `maxRetries` em HostExtensions |
| Dados não aparecem | Verificar `ASPNETCORE_ENVIRONMENT=Development` |
| Timeout | Aumentar `delayMilliseconds` |

## 📝 Modificar Dados de Seed

1. Edite `OficinaDbContextSeeder.cs`
2. Adicione novo cliente/veículo/etc
3. Recrie containers: `docker-compose down -v && docker-compose up --build`


