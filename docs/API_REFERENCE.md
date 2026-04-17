# Referência da API

## Base URL

```text
http://localhost:8080/api/v1
```

## Swagger

Em ambiente `Development`, a especificação interativa fica disponível em:

```text
http://localhost:8080/swagger
```

## Autenticação

### Login

Endpoint:

```text
POST /auth/login
```

Payload:

```json
{
  "usuario": "admin",
  "senha": "admin123"
}
```

Resposta:

```json
{
  "token": "<jwt>",
  "expiraEm": "2026-04-17T15:00:00Z",
  "nomeUsuario": "Administrador",
  "role": "Administrador"
}
```

Uso do token:

```http
Authorization: Bearer <jwt>
```

## Controle de acesso

### Endpoints protegidos por JWT

- `/clientes`
- `/veiculos`
- `/pecas`

### Endpoints atualmente públicos

- `/auth/login`
- `/servicos`
- `/ordens-servico`

## Recursos

### Clientes

```text
POST   /clientes
GET    /clientes
GET    /clientes/{id}
GET    /clientes/documento/{cpfCnpj}
PUT    /clientes/{id}
DELETE /clientes/{id}
```

### Veículos

```text
POST   /veiculos
GET    /veiculos
GET    /veiculos/{id}
GET    /veiculos/placa/{placa}
GET    /veiculos/cliente/{clienteId}
PUT    /veiculos/{id}
DELETE /veiculos/{id}
```

### Serviços

```text
POST   /servicos
GET    /servicos
GET    /servicos/{id}
PUT    /servicos/{id}
DELETE /servicos/{id}
```

### Peças

```text
POST   /pecas
GET    /pecas
GET    /pecas/{id}
PUT    /pecas/{id}
DELETE /pecas/{id}
```

### Ordens de serviço

```text
POST   /ordens-servico
GET    /ordens-servico
GET    /ordens-servico/{id}
GET    /ordens-servico/cliente/{clienteId}
GET    /ordens-servico/status/{status}
PUT    /ordens-servico/{id}/status
POST   /ordens-servico/{id}/servicos
POST   /ordens-servico/{id}/pecas
DELETE /ordens-servico/{id}
```

## Contratos principais

### Criar cliente

```json
{
  "nome": "Maria Silva",
  "cpfCnpj": "12345678901",
  "telefone": "11999999999",
  "email": "maria@example.com"
}
```

### Criar veículo

```json
{
  "placa": "ABC1234",
  "marca": "Toyota",
  "modelo": "Corolla",
  "ano": 2020,
  "clienteId": 1
}
```

### Criar serviço

```json
{
  "nome": "Troca de Oleo",
  "descricao": "Troca de oleo do motor e filtro",
  "preco": 150.0,
  "tempoEstimado": 30
}
```

### Criar peça

```json
{
  "nome": "Filtro de Oleo",
  "preco": 45.0,
  "quantidadeEstoque": 10
}
```

### Criar ordem de serviço

```json
{
  "clienteId": 1,
  "veiculoId": 1
}
```

### Atualizar status da ordem

```json
{
  "novoStatus": "EmExecucao"
}
```

### Adicionar serviço à ordem

```json
{
  "servicoId": 1
}
```

### Adicionar peça à ordem

```json
{
  "pecaId": 1,
  "quantidade": 2
}
```

## Regras de domínio relevantes

### Ordem de serviço

- a ordem possui fluxo de status restrito;
- a alteração de status inválida retorna erro de negócio;
- serviços só podem ser adicionados quando a ordem está em `AguardandoAprovacao`;
- peças podem ser adicionadas em `AguardandoAprovacao` ou `EmExecucao`;
- ao adicionar peças, o estoque é decrementado;
- o valor total da ordem é recalculado a partir de serviços e peças.

### Tratamento de erro

O filtro global traduz:

- `KeyNotFoundException` em `404 Not Found`;
- `ArgumentException` e `InvalidOperationException` em `400 Bad Request`.

## Fluxo sugerido de uso

1. Fazer login em `/auth/login`.
2. Criar ou consultar um cliente.
3. Criar ou consultar um veículo vinculado ao cliente.
4. Criar uma ordem de serviço.
5. Consultar serviços e peças disponíveis.
6. Atualizar o status da ordem ao longo da execução.

## Observação

Para contratos completos de request e response, priorize o Swagger da aplicação. Esta página resume a superfície da API e os fluxos mais importantes.
