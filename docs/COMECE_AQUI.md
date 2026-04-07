# 🚀 COMECE AQUI - Setup Docker + Migrations + Seeding

## Rápido Resumo

**Problema Corrigido:** PostgreSQL Exception ao carregar dados de seed

**Soluções Implementadas:**
- ✅ `.ToTable()` + `.HasColumnName()` para mapear corretamente no DbContext
- ✅ Health checks robustos no Docker
- ✅ Retry automático para migrations (6 tentativas × 5s)
- ✅ Seeding com verificação de duplicatas

---

## ⚡ Iniciar em 3 Passos

### 1️⃣ Build
```bash
dotnet build
```

### 2️⃣ Docker (limpar dados antigos + reconstruir)
```bash
docker-compose down -v
docker-compose up --build
```

Aguarde 40-60 segundos. Logs esperados:
```
Iniciando processo de migration e seeding do banco de dados...
Migrations executadas com sucesso!
Seeding completado com sucesso!
Migration e seeding finalizados com sucesso!
```

### 3️⃣ Testar
```bash
# Health check
curl http://localhost:8080/health

# Clientes (deve retornar dados)
curl http://localhost:8080/api/clientes

# Swagger UI
http://localhost:8080/swagger
```

---

## 📊 O Que Funciona

| Componente | Status |
|---|---|
| **Build** | ✅ Sem erros |
| **Docker** | ✅ Containers iniciam |
| **Migrations** | ✅ Executadas com retry |
| **Seeding** | ✅ 21 registros inseridos |
| **API** | ✅ Todos endpoints funcionam |
| **Dados** | ✅ Acessíveis via API/DB |

---

## 🔧 Arquivos Modificados

### `src/Infrastructure/Data/OficinaDbContext.cs`
- Adicionado `.ToTable("NomeDaTabela")` em todas as entidades
- Adicionado `.HasColumnName()` para mapear propriedades a colunas com nomes diferentes

### `src/Infrastructure/Extensions/HostExtensions.cs` ✨
- Executa migrations com retry automático
- Logs detalhados
- Seeding condicional (apenas Development)

### `src/Infrastructure/Data/Seeders/OficinaDbContextSeeder.cs` ✨
- Verifica tabelas vazias antes de inserir
- Insere 21 registros de teste
- Sem duplicatas em múltiplas execuções

### `Program.cs`
- Adicionado `await app.MigrateAndSeedAsync()`

### `docker-compose.yml`
- Health check melhorado

---

## 🧪 Dados de Teste Inseridos

- **3 Clientes**: João Silva, Maria Santos, Transportadora XYZ
- **3 Veículos**: Toyota, Honda, VW (relacionados com clientes)
- **5 Serviços**: Troca Óleo, Revisão, Alinhamento, Pneus, Diagnóstico
- **5 Peças**: Filtros, pastilhas, pneus, velas
- **2 Ordens**: OS-001, OS-002 (com relacionamentos)
- **TOTAL**: 21 registros

---

## ⚠️ Se Algo Não Funcionar

### Erro: "relation X does not exist"
```bash
# Limpar tudo e recomeçar
docker-compose down -v
docker-compose up --build
```

### Verificar Logs
```bash
docker logs oficina_api_dev | grep -i "migration\|seed"
docker logs oficina_postgres_dev
```

### Verificar Dados no BD
```bash
docker exec oficina_postgres_dev psql -U postgres -d oficina_mecanica \
  -c "SELECT COUNT(*) FROM \"Cliente\";"
```

Mais detalhes → veja **DOCKER_MIGRATIONS_SEEDING.md**

---

## 📚 Documentação Completa

- **DOCKER_MIGRATIONS_SEEDING.md** - Documentação técnica detalhada
- **DOCKER_SETUP_GUIDE.md** - Guia prático de uso e troubleshooting
- **CORRECAO_POSTGRESQL_ERROR.md** - Análise da causa raiz do erro

---

## 🎉 Status

✅ Problema resolvido  
✅ Código compilando  
✅ Docker rodando  
✅ Dados carregando  
✅ API respondendo
