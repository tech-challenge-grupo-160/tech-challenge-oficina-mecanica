# 🧪 Exemplos de Uso - API REST

Base URL: `http://localhost:8080/api`

---

## 📝 Clientes

### 1. Criar Cliente
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

**Resposta (201):**
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

### 2. Listar Clientes
```bash
curl -X GET http://localhost:8080/api/clientes
```

**Resposta:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "nome": "João Silva",
    "cpfCnpj": "12345678901",
    "telefone": "11999999999",
    "email": "joao@example.com",
    "dataCadastro": "2026-03-16T22:30:00Z"
  }
]
```

### 3. Obter Cliente por ID
```bash
curl -X GET http://localhost:8080/api/clientes/550e8400-e29b-41d4-a716-446655440000
```

### 4. Atualizar Cliente
```bash
curl -X PUT http://localhost:8080/api/clientes/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Silva Atualizado",
    "telefone": "11988888888",
    "email": "joao.novo@example.com"
  }'
```

### 5. Deletar Cliente
```bash
curl -X DELETE http://localhost:8080/api/clientes/550e8400-e29b-41d4-a716-446655440000
```

---

## 🚗 Veículos

### 1. Criar Veículo
```bash
curl -X POST http://localhost:8080/api/veiculos \
  -H "Content-Type: application/json" \
  -d '{
    "placa": "ABC1D23",
    "marca": "Toyota",
    "modelo": "Corolla",
    "ano": 2023,
    "clienteId": "550e8400-e29b-41d4-a716-446655440000"
  }'
```

**Resposta:**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "placa": "ABC1D23",
  "marca": "Toyota",
  "modelo": "Corolla",
  "ano": 2023,
  "clienteId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 2. Listar Veículos de um Cliente
```bash
curl -X GET http://localhost:8080/api/veiculos/cliente/550e8400-e29b-41d4-a716-446655440000
```

### 3. Atualizar Veículo
```bash
curl -X PUT http://localhost:8080/api/veiculos/660e8400-e29b-41d4-a716-446655440001 \
  -H "Content-Type: application/json" \
  -d '{
    "marca": "Toyota",
    "modelo": "Corolla XE",
    "ano": 2024
  }'
```

---

## 🔧 Serviços

### 1. Criar Serviço
```bash
curl -X POST http://localhost:8080/api/servicos \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Revisão Completa",
    "descricao": "Revisão de motor, freios e fluidos",
    "preco": 150.00,
    "tempoEstimado": 120
  }'
```

**Resposta:**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440002",
  "nome": "Revisão Completa",
  "descricao": "Revisão de motor, freios e fluidos",
  "preco": 150.00,
  "tempoEstimado": 120
}
```

### 2. Listar Serviços
```bash
curl -X GET http://localhost:8080/api/servicos
```

### 3. Criar Mais Serviços
```bash
# Troca de óleo
curl -X POST http://localhost:8080/api/servicos \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Troca de Óleo",
    "descricao": "Troca de óleo e filtro",
    "preco": 75.00,
    "tempoEstimado": 30
  }'

# Balanceamento
curl -X POST http://localhost:8080/api/servicos \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Balanceamento de Rodas",
    "descricao": "Balanceamento das 4 rodas",
    "preco": 100.00,
    "tempoEstimado": 60
  }'
```

---

## 🔩 Peças

### 1. Criar Peça
```bash
curl -X POST http://localhost:8080/api/pecas \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Filtro de Óleo",
    "preco": 25.00,
    "quantidadeEstoque": 50
  }'
```

**Resposta:**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440003",
  "nome": "Filtro de Óleo",
  "preco": 25.00,
  "quantidadeEstoque": 50
}
```

### 2. Criar Mais Peças
```bash
# Correia
curl -X POST http://localhost:8080/api/pecas \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Correia Dentada",
    "preco": 150.00,
    "quantidadeEstoque": 20
  }'

# Bateria
curl -X POST http://localhost:8080/api/pecas \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Bateria 60Ah",
    "preco": 400.00,
    "quantidadeEstoque": 15
  }'
```

### 3. Listar Peças
```bash
curl -X GET http://localhost:8080/api/pecas
```

---

## 📋 Ordens de Serviço

### 1. Criar Ordem de Serviço
```bash
curl -X POST http://localhost:8080/api/ordens-servico \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": "550e8400-e29b-41d4-a716-446655440000",
    "veiculoId": "660e8400-e29b-41d4-a716-446655440001"
  }'
```

**Resposta:**
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440004",
  "numero": "OS-20260316-ABC12345",
  "clienteId": "550e8400-e29b-41d4-a716-446655440000",
  "veiculoId": "660e8400-e29b-41d4-a716-446655440001",
  "status": "Recebida",
  "dataAbertura": "2026-03-16T22:30:00Z",
  "dataConclusao": null,
  "valorTotal": 0.00,
  "servicos": [],
  "pecas": []
}
```

### 2. Atualizar Status para EmDiagnostico
```bash
curl -X PUT http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004/status \
  -H "Content-Type: application/json" \
  -d '{
    "novoStatus": "EmDiagnostico"
  }'
```

### 3. Atualizar Status para AguardandoAprovacao
```bash
curl -X PUT http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004/status \
  -H "Content-Type: application/json" \
  -d '{
    "novoStatus": "AguardandoAprovacao"
  }'
```

### 4. Adicionar Serviço à Ordem
```bash
curl -X POST http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004/servicos \
  -H "Content-Type: application/json" \
  -d '{
    "servicoId": "770e8400-e29b-41d4-a716-446655440002"
  }'
```

**Resposta (ordem atualizada com serviço):**
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440004",
  "numero": "OS-20260316-ABC12345",
  "clienteId": "550e8400-e29b-41d4-a716-446655440000",
  "veiculoId": "660e8400-e29b-41d4-a716-446655440001",
  "status": "AguardandoAprovacao",
  "dataAbertura": "2026-03-16T22:30:00Z",
  "dataConclusao": null,
  "valorTotal": 150.00,
  "servicos": [
    {
      "servicoId": "770e8400-e29b-41d4-a716-446655440002",
      "preco": 150.00,
      "tempoEstimado": 120
    }
  ],
  "pecas": []
}
```

### 5. Adicionar Peça à Ordem
```bash
curl -X POST http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004/pecas \
  -H "Content-Type: application/json" \
  -d '{
    "pecaId": "880e8400-e29b-41d4-a716-446655440003",
    "quantidade": 2
  }'
```

**Resposta:**
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440004",
  "numero": "OS-20260316-ABC12345",
  "status": "AguardandoAprovacao",
  "valorTotal": 200.00,
  "servicos": [...],
  "pecas": [
    {
      "pecaId": "880e8400-e29b-41d4-a716-446655440003",
      "quantidade": 2,
      "preco": 25.00
    }
  ]
}
```

### 6. Prosseguir com Ordem (Atualizar Status)
```bash
# EmExecucao
curl -X PUT http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus": "EmExecucao"}'

# Finalizada
curl -X PUT http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus": "Finalizada"}'

# Entregue
curl -X PUT http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus": "Entregue"}'
```

### 7. Listar Ordens por Status
```bash
# Recebidas
curl -X GET http://localhost:8080/api/ordens-servico/status/Recebida

# Em Execução
curl -X GET http://localhost:8080/api/ordens-servico/status/EmExecucao

# Entregues
curl -X GET http://localhost:8080/api/ordens-servico/status/Entregue
```

### 8. Listar Ordens por Cliente
```bash
curl -X GET http://localhost:8080/api/ordens-servico/cliente/550e8400-e29b-41d4-a716-446655440000
```

### 9. Obter Ordem Completa
```bash
curl -X GET http://localhost:8080/api/ordens-servico/990e8400-e29b-41d4-a716-446655440004
```

---

## 🏥 Health Checks

### Verificar Saúde da Aplicação
```bash
curl -X GET http://localhost:8080/health
```

**Resposta:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Database",
      "status": "Healthy",
      "description": "Healthy",
      "duration": 125.45
    }
  ]
}
```

### Liveness Probe (Container Running)
```bash
curl -X GET http://localhost:8080/health/live
```

### Readiness Probe (Ready to Serve)
```bash
curl -X GET http://localhost:8080/health/ready
```

---

## 📊 Fluxo Completo de Exemplo

### Passo 1: Criar Cliente
```bash
CLIENT_ID=$(curl -s -X POST http://localhost:8080/api/clientes \
  -H "Content-Type: application/json" \
  -d '{"nome":"João Silva","cpfCnpj":"12345678901","telefone":"11999999999","email":"joao@example.com"}' \
  | jq -r '.id')

echo "Cliente criado: $CLIENT_ID"
```

### Passo 2: Criar Veículo
```bash
VEHICLE_ID=$(curl -s -X POST http://localhost:8080/api/veiculos \
  -H "Content-Type: application/json" \
  -d "{\"placa\":\"ABC1D23\",\"marca\":\"Toyota\",\"modelo\":\"Corolla\",\"ano\":2023,\"clienteId\":\"$CLIENT_ID\"}" \
  | jq -r '.id')

echo "Veículo criado: $VEHICLE_ID"
```

### Passo 3: Criar Serviço
```bash
SERVICE_ID=$(curl -s -X POST http://localhost:8080/api/servicos \
  -H "Content-Type: application/json" \
  -d '{"nome":"Revisão Completa","descricao":"Revisão de motor, freios e fluidos","preco":150.00,"tempoEstimado":120}' \
  | jq -r '.id')

echo "Serviço criado: $SERVICE_ID"
```

### Passo 4: Criar Peça
```bash
PART_ID=$(curl -s -X POST http://localhost:8080/api/pecas \
  -H "Content-Type: application/json" \
  -d '{"nome":"Filtro de Óleo","preco":25.00,"quantidadeEstoque":50}' \
  | jq -r '.id')

echo "Peça criada: $PART_ID"
```

### Passo 5: Criar Ordem de Serviço
```bash
ORDER_ID=$(curl -s -X POST http://localhost:8080/api/ordens-servico \
  -H "Content-Type: application/json" \
  -d "{\"clienteId\":\"$CLIENT_ID\",\"veiculoId\":\"$VEHICLE_ID\"}" \
  | jq -r '.id')

echo "Ordem criada: $ORDER_ID"
```

### Passo 6: Fluxo de Status
```bash
# Recebida -> EmDiagnostico
curl -s -X PUT http://localhost:8080/api/ordens-servico/$ORDER_ID/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus":"EmDiagnostico"}' | jq '.status'

# EmDiagnostico -> AguardandoAprovacao
curl -s -X PUT http://localhost:8080/api/ordens-servico/$ORDER_ID/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus":"AguardandoAprovacao"}' | jq '.status'
```

### Passo 7: Adicionar Itens à Ordem
```bash
# Adicionar serviço
curl -s -X POST http://localhost:8080/api/ordens-servico/$ORDER_ID/servicos \
  -H "Content-Type: application/json" \
  -d "{\"servicoId\":\"$SERVICE_ID\"}" | jq '.valorTotal'

# Adicionar peça (2 unidades)
curl -s -X POST http://localhost:8080/api/ordens-servico/$ORDER_ID/pecas \
  -H "Content-Type: application/json" \
  -d "{\"pecaId\":\"$PART_ID\",\"quantidade\":2}" | jq '.valorTotal'
```

### Passo 8: Finalizar Ordem
```bash
# EmExecucao
curl -s -X PUT http://localhost:8080/api/ordens-servico/$ORDER_ID/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus":"EmExecucao"}' | jq '.status'

# Finalizada
curl -s -X PUT http://localhost:8080/api/ordens-servico/$ORDER_ID/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus":"Finalizada"}' | jq '.status'

# Entregue
curl -s -X PUT http://localhost:8080/api/ordens-servico/$ORDER_ID/status \
  -H "Content-Type: application/json" \
  -d '{"novoStatus":"Entregue"}' | jq '{status: .status, dataConclusao: .dataConclusao}'
```

---

## 🎯 Testes com Postman/Insomnia

Importe a seguinte coleção JSON:

```json
{
  "info": {
    "name": "Oficina Mecânica API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Clientes",
      "item": [
        {"name": "Criar", "request": {"method": "POST", "url": "http://localhost:8080/api/clientes"}},
        {"name": "Listar", "request": {"method": "GET", "url": "http://localhost:8080/api/clientes"}}
      ]
    },
    {
      "name": "Veículos",
      "item": [
        {"name": "Criar", "request": {"method": "POST", "url": "http://localhost:8080/api/veiculos"}},
        {"name": "Listar", "request": {"method": "GET", "url": "http://localhost:8080/api/veiculos"}}
      ]
    },
    {
      "name": "Ordens de Serviço",
      "item": [
        {"name": "Criar", "request": {"method": "POST", "url": "http://localhost:8080/api/ordens-servico"}},
        {"name": "Listar", "request": {"method": "GET", "url": "http://localhost:8080/api/ordens-servico"}}
      ]
    }
  ]
}
```

---

## 📌 Dicas Úteis

### Usar jq para parsing JSON
```bash
curl ... | jq '.id'          # Extrair ID
curl ... | jq '.[] | .nome'  # Listar nomes
curl ... | jq 'length'       # Contar elementos
```

### Variáveis de ambiente em bash
```bash
export API_URL="http://localhost:8080/api"
curl -X GET $API_URL/clientes
```

### Pretty print JSON
```bash
curl ... | jq '.'
```

---

**Todos os exemplos estão prontos para uso! 🚀**
