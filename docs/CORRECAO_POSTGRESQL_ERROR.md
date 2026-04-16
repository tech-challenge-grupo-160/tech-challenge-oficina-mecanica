# Correção: PostgreSQL Exception - Análise da Causa Raiz

## Problema
```
Npgsql.PostgresException: 42P01: relation "Clientes" does not exist
Npgsql.PostgresException: 42703: column o0.OrdemDeServicoId does not exist
```

## Causa Raiz

### 1. Conflito de Case-Sensitivity
- **init-db.sql** criava: `"Cliente"`, `"Veiculo"`, `"Servico"` (singulares)
- **DbContext** (antes) esperava: `cliente`, `veiculo` (sem `.ToTable()`)
- **PostgreSQL** com aspas = case-sensitive → **ERRO 42P01** ❌

### 2. Nomes de Colunas Diferentes
- **init-db.sql**: coluna `TempoEstimadoMinutos`
- **DbContext Servico**: propriedade `TempoEstimado`
- Sem `.HasColumnName()` → **ERRO 42703** ❌

### 3. Nomes de Entidades vs. Tabelas
- **init-db.sql**: `OrdemServicoItemServico` (tabela)
- **DbContext**: `OrdemDeServicoServico` (classe C#)
- Sem `.ToTable()` → mapeamento errado ❌

## Solução

### Adicionar a `OficinaDbContext.cs`

```csharp
// 1. Mapear tabela
entity.ToTable("NomeDaTabela");

// 2. Mapear colunas com nomes diferentes
entity.Property(e => e.TempoEstimado)
    .HasColumnName("TempoEstimadoMinutos");

// 3. Para chaves primárias compostas
entity.Property(e => e.OrdemDeServicoId)
    .HasColumnName("OrdemServicoId");
```

### Mapeamentos Aplicados

| Classe C# | Tabela | Propriedade | Coluna |
|-----------|--------|-----------|--------|
| Cliente | Cliente | - | - |
| Veiculo | Veiculo | - | - |
| Servico | Servico | TempoEstimado | TempoEstimadoMinutos |
| Peca | Peca | - | - |
| OrdemDeServico | OrdemServico | - | - |
| OrdemDeServicoServico | OrdemServicoItemServico | OrdemDeServicoId | OrdemServicoId |
| OrdemDeServicoServico | OrdemServicoItemServico | TempoEstimado | TempoEstimadoMinutos |
| OrdemDeServicoPeca | OrdemServicoItemPeca | OrdemDeServicoId | OrdemServicoId |

## Lições Aprendidas

1. **PostgreSQL é case-sensitive** quando usa aspas duplas
2. **Sempre use `.ToTable()` explícito** para evitar surpresas
3. **Use `.HasColumnName()` quando nomes diferem** entre C# e banco
4. **Não deixe convenções implícitas** - seja explícito!
5. **Migrations e DbContext devem estar sincronizados**

## Verificação

```bash
# Verificar mapeamento está correto
docker exec oficina-mecanica-postgres-dev psql -U postgres -d oficina_mecanica -c "\d+ \"Cliente\""

# Contar registros
docker exec oficina-mecanica-postgres-dev psql -U postgres -d oficina_mecanica \
  -c "SELECT COUNT(*) FROM \"Cliente\";"

# Listar tabelas
docker exec oficina-mecanica-postgres-dev psql -U postgres -d oficina_mecanica -c "\dt"
```

Status: ✅ **CORRIGIDO**

