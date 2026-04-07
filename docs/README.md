# 📚 Documentação - Índice

## 🚀 Comece Aqui

Se está iniciando ou precisa rodar a aplicação rapidamente:

### [`docs/COMECE_AQUI.md`](docs/COMECE_AQUI.md) ⭐
- Quick start em 3 passos
- Comando Docker
- Verificações rápidas
- Erros comuns

## 📖 Documentação Técnica

### [`docs/DOCKER_MIGRATIONS_SEEDING.md`](docs/DOCKER_MIGRATIONS_SEEDING.md)
- Arquitetura de migrations
- Configuração de seeding
- Componentes principais
- Troubleshooting técnico

### [`docs/DOCKER_SETUP_GUIDE.md`](docs/DOCKER_SETUP_GUIDE.md)
- Guia prático de operação
- Como acessar serviços
- Verificação de dados
- Modificar seed

### [`docs/CORRECAO_POSTGRESQL_ERROR.md`](docs/CORRECAO_POSTGRESQL_ERROR.md)
- Análise de erro PostgreSQL
- Causa raiz
- Soluções aplicadas
- Lições aprendidas

---

## 📊 Estrutura da Solução

```
src/Infrastructure/
├── Extensions/
│   └── HostExtensions.cs ..................... Migrations com retry
├── Data/
│   ├── OficinaDbContext.cs ................... Mapeamentos EF Core
│   └── Seeders/
│       └── OficinaDbContextSeeder.cs ......... Dados de teste
└── ...

Program.cs .................................. Chamada MigrateAndSeedAsync()
docker-compose.yml ........................... Health checks + orchestração
```

## ✅ O Que Funciona

- ✅ Migrations automáticas com retry
- ✅ Seeding sem duplicatas (21 registros)
- ✅ Health checks robustos
- ✅ API endpoints funcionando
- ✅ Dados acessíveis

## 🎯 Fluxo de Inicialização

```
docker-compose up --build
         ↓
PostgreSQL inicia + healthcheck
         ↓
API aguarda PostgreSQL saudável
         ↓
Program.cs → MigrateAndSeedAsync()
         ↓
    ├─ MigrateWithRetryAsync()    [6 tentativas × 5s]
    └─ SeedAsync()                 [21 registros]
         ↓
API pronta em http://localhost:8080/swagger
```

## 🔍 Verificação Rápida

```bash
# Health check
curl http://localhost:8080/health

# API funcionando
curl http://localhost:8080/api/clientes

# Contar registros no banco
docker exec oficina_postgres_dev psql -U postgres -d oficina_mecanica \
  -c "SELECT COUNT(*) FROM \"Cliente\";"
```

## ⚠️ Erros Comuns

| Erro | Solução |
|------|---------|
| "relation X does not exist" | `docker-compose down -v && docker-compose up --build` |
| Timeout em startup | Aumentar `maxRetries` em `HostExtensions.cs` |
| Dados não carregam | Verificar `ASPNETCORE_ENVIRONMENT=Development` |

## 📞 Suporte

Leia a documentação conforme necessário:
- **Problema ao iniciar?** → `COMECE_AQUI.md`
- **Erro PostgreSQL?** → `CORRECAO_POSTGRESQL_ERROR.md`
- **Como operar?** → `DOCKER_SETUP_GUIDE.md`
- **Detalhes técnicos?** → `DOCKER_MIGRATIONS_SEEDING.md`
