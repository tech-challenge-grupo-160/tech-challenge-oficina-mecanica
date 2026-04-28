# Referência da API

## Base URL

```text
http://localhost:8080/api/v1
```

## Swagger

Disponível apenas em `Development`:

```text
http://localhost:8080/swagger
```

## Autenticação

### Login

```http
POST /auth/login
```

Payload:

```json
{
  "usuario": "admin",
  "senha": "admin123"
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
- `/ordens-servico`
- `/pedidos-compra`

### Endpoints públicos

- `/auth/login`
- `/servicos`

## Recursos

### Clientes

```text
POST   /clientes
GET    /clientes?page=1&pageSize=10&nome=&cpfCnpj=
GET    /clientes/documento/{cpfCnpj}
GET    /clientes/{cpfCnpj}/veiculos
POST   /clientes/{cpfCnpj}/veiculos
PUT    /clientes/documento/{cpfCnpj}
DELETE /clientes/documento/{cpfCnpj}
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
GET    /ordens-servico?page=1&pageSize=10&clienteId=&status=&numero=&dataAberturaInicio=&dataAberturaFim=
GET    /ordens-servico/{id}
GET    /ordens-servico/{id}/historico
GET    /ordens-servico/{id}/movimentacoes-estoque
GET    /ordens-servico/{id}/monitoramento
GET    /ordens-servico/monitoramento?page=1&pageSize=10
GET    /ordens-servico/cliente/{clienteId}
GET    /ordens-servico/status/{status}
PATCH  /ordens-servico/{id}/iniciar-diagnostico
PATCH  /ordens-servico/{id}/finalizar-diagnostico
PATCH  /ordens-servico/{id}/aprovar
PATCH  /ordens-servico/{id}/liberar-execucao
PATCH  /ordens-servico/{id}/finalizar
PATCH  /ordens-servico/{id}/registrar-pagamento
PATCH  /ordens-servico/{id}/entregar
PATCH  /ordens-servico/{id}/cancelar
POST   /ordens-servico/{id}/servicos
DELETE /ordens-servico/{id}/servicos/{servicoId}
POST   /ordens-servico/{id}/pecas
DELETE /ordens-servico/{id}/pecas/{pecaId}
```

### Pedidos de compra

```text
POST   /pedidos-compra
GET    /pedidos-compra?page=1&pageSize=10
GET    /pedidos-compra/ordem/{ordemDeServicoId}
PATCH  /pedidos-compra/{id}/receber
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
  "placa": "ABC1D23",
  "marca": "Toyota",
  "modelo": "Corolla",
  "ano": 2020,
  "cpfCnpj": "12345678901"
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
  "marca": "Bosch",
  "modelo": "F-0001",
  "preco": 45.0,
  "quantidadeEstoque": 10
}
```

### Criar ordem de serviço

```json
{
  "clienteId": 1000,
  "veiculoId": 1000,
  "descricaoSolicitacao": "Cliente relatou ruido ao frear.",
  "observacoesRecepcao": "Problema ocorre em baixa velocidade."
}
```

### Adicionar serviço à ordem

```json
{
  "servicoId": 1000
}
```

### Adicionar peça à ordem

```json
{
  "pecaId": 1000,
  "quantidade": 2
}
```

### Cancelar ordem

```json
{
  "motivoCancelamento": "Cliente desistiu do reparo."
}
```

### Criar pedido de compra

```json
{
  "ordemDeServicoId": 3001,
  "pecaId": 1000,
  "quantidadeSolicitada": 4,
  "observacao": "Pedido manual para reposicao"
}
```

### Registrar recebimento do pedido

```json
{
  "quantidadeRecebida": 4
}
```

## Fluxo da ordem de serviço

Estados implementados:

```text
Recebida
EmDiagnostico
AguardandoAprovacao
EmExecucao
Finalizada
Entregue
Cancelada
AguardandoEstoque
```

Fluxo principal:

```text
Recebida -> EmDiagnostico -> AguardandoAprovacao -> EmExecucao -> Finalizada -> Entregue
                                   \-> AguardandoEstoque -> EmExecucao
```

## Monitoramento

### Resumo paginado

```http
GET /ordens-servico/monitoramento?page=1&pageSize=10
```

Retorna:

- `TotalOrdens`
- `TotalOrdensAbertas`
- `TotalOrdensFinalizadas`
- `Page`
- `PageSize`
- `TotalPages`
- `TempoMedioFinalizacaoMinutos`
- `TempoMedioFinalizacaoHoras`
- `Ordens`

### Monitoramento por OS

```http
GET /ordens-servico/{id}/monitoramento
```

Retorna:

- status atual;
- data de abertura;
- data de finalização, quando houver;
- tempo decorrido;
- tempo total de finalização, quando aplicável.

## Estoque e compras

Regra central:

- a transição `AguardandoAprovacao -> EmExecucao` só ocorre após validação de estoque;
- se houver disponibilidade total, o sistema baixa estoque e registra movimentação;
- se faltar item, a OS vai para `AguardandoEstoque` e o sistema pode gerar pedido de compra;
- `/aprovar` não aceita OS em `AguardandoEstoque`;
- após o recebimento do pedido e reposição do estoque, use `PATCH /ordens-servico/{id}/liberar-execucao`.

Rastreabilidade:

- `GET /ordens-servico/{id}/historico`
- `GET /ordens-servico/{id}/movimentacoes-estoque`
- `GET /pedidos-compra/ordem/{ordemDeServicoId}`
- `GET /pedidos-compra?page=1&pageSize=10`

## Regras de domínio relevantes

- a OS só pode iniciar diagnóstico quando está em `Recebida`;
- o diagnóstico só pode ser finalizado se houver ao menos um serviço e orçamento maior que zero;
- serviços só podem ser adicionados e removidos em `EmDiagnostico`;
- repetir o mesmo serviço na OS retorna erro de negócio;
- peças só podem ser removidas em `EmDiagnostico`;
- peças podem ser adicionadas em `EmDiagnostico`, `AguardandoAprovacao` e `AguardandoEstoque`;
- a OS não entra em `EmExecucao` sem validação de estoque bem-sucedida;
- uma OS em `AguardandoEstoque` só pode ir para `EmExecucao` pela rota de liberação de execução;
- pagamento só pode ser registrado após finalização;
- entrega só pode ocorrer após pagamento.

## Tratamento de erros

O filtro global traduz:

- `KeyNotFoundException` -> `404 Not Found`
- `InvalidOperationException` -> `400 Bad Request`
- `ArgumentException` -> `400 Bad Request`

Formato padrão:

```json
{
  "message": "Mensagem de erro de dominio."
}
```
