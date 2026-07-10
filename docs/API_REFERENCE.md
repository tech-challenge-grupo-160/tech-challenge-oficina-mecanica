# Referencia da API

Ultima atualizacao: 2026-07-10

## Base URL

```text
http://localhost:8080/api/v1
```

## Swagger

Disponivel apenas em `Development`:

```text
http://localhost:8080/swagger
```

## Autenticacao

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

Resposta `200 OK`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
  "expiraEm": "2026-07-10T14:00:00",
  "nomeUsuario": "admin",
  "role": "Admin"
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
- `/servicos`
- `/pecas`
- `/ordens-servico`
- `/pedidos-compra`

### Endpoints publicos

- `POST /auth/login`
- `GET /acompanhamento-os/{codigo}` (requer header `X-Tracking-Token`)
- `POST /ordens-servico/{id}/ordem/resposta` (requer header `X-Tracking-Token`)

## Tratamento de erros

O filtro global traduz excecoes em respostas padronizadas:

| Excecao | Status HTTP |
|---|---|
| `ServiceNotFoundException` | `404 Not Found` |
| `KeyNotFoundException` | `404 Not Found` |
| `ServiceValidationException` | `422 Unprocessable Entity` |
| `InvalidOperationException` | `400 Bad Request` |
| `ArgumentException` | `400 Bad Request` |
| `UnauthorizedAccessException` | `401 Unauthorized` |
| Demais excecoes | `500 Internal Server Error` |

Formato padrao:

```json
{
  "message": "Mensagem de erro."
}
```

## Paginacao

Endpoints paginados retornam o envelope `PagedResponse<T>`:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalItems": 42,
  "totalPages": 5
}
```

---

## Recursos

### Auth

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/auth/login` | Publica | Autenticar usuario |

### Clientes

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/clientes` | JWT | Criar cliente |
| `GET` | `/clientes?page=&pageSize=&nome=&cpfCnpj=` | JWT | Listar clientes (paginado) |
| `GET` | `/clientes/documento/{cpfCnpj}` | JWT | Obter cliente por CPF/CNPJ |
| `GET` | `/clientes/{cpfCnpj}/veiculos` | JWT | Listar veiculos do cliente por documento |
| `POST` | `/clientes/{cpfCnpj}/veiculos` | JWT | Criar veiculo vinculado ao cliente |
| `PUT` | `/clientes/documento/{cpfCnpj}` | JWT | Atualizar cliente por CPF/CNPJ |
| `DELETE` | `/clientes/documento/{cpfCnpj}` | JWT | Deletar cliente por CPF/CNPJ |

#### Criar cliente

Request:

```json
{
  "nome": "Maria Silva",
  "cpfCnpj": "12345678901",
  "telefone": "11999999999",
  "email": "maria@example.com"
}
```

Resposta `201 Created`:

```json
{
  "id": 1000,
  "nome": "Maria Silva",
  "cpfCnpj": "12345678901",
  "telefone": "11999999999",
  "email": "maria@example.com",
  "dataCadastro": "2026-07-10T10:00:00"
}
```

### Veiculos

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/veiculos` | JWT | Criar veiculo |
| `GET` | `/veiculos` | JWT | Listar todos os veiculos |
| `GET` | `/veiculos/{id}` | JWT | Obter veiculo por ID |
| `GET` | `/veiculos/placa/{placa}` | JWT | Obter veiculo por placa |
| `GET` | `/veiculos/cliente/{clienteId}` | JWT | Listar veiculos por cliente |
| `PUT` | `/veiculos/{id}` | JWT | Atualizar veiculo |
| `DELETE` | `/veiculos/{id}` | JWT | Deletar veiculo |

#### Criar veiculo

Request:

```json
{
  "placa": "ABC1D23",
  "marca": "Toyota",
  "modelo": "Corolla",
  "ano": 2020,
  "cpfCnpj": "12345678901"
}
```

Resposta `201 Created`:

```json
{
  "id": 1000,
  "placa": "ABC1D23",
  "marca": "Toyota",
  "modelo": "Corolla",
  "ano": 2020,
  "clienteId": 1000
}
```

### Servicos

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/servicos` | JWT | Criar servico |
| `GET` | `/servicos` | JWT | Listar todos os servicos |
| `GET` | `/servicos/{id}` | JWT | Obter servico por ID |
| `PUT` | `/servicos/{id}` | JWT | Atualizar servico |
| `DELETE` | `/servicos/{id}` | JWT | Deletar servico |

#### Criar servico

Request:

```json
{
  "nome": "Troca de Oleo",
  "descricao": "Troca de oleo do motor e filtro",
  "preco": 150.0,
  "tempoEstimado": 30
}
```

Resposta `201 Created`:

```json
{
  "id": 1000,
  "nome": "Troca de Oleo",
  "descricao": "Troca de oleo do motor e filtro",
  "preco": 150.0,
  "tempoEstimado": 30
}
```

### Pecas

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/pecas` | JWT | Criar peca |
| `GET` | `/pecas` | JWT | Listar todas as pecas |
| `GET` | `/pecas/{id}` | JWT | Obter peca por ID |
| `PUT` | `/pecas/{id}` | JWT | Atualizar peca |
| `DELETE` | `/pecas/{id}` | JWT | Deletar peca |

#### Criar peca

Request:

```json
{
  "nome": "Filtro de Oleo",
  "marca": "Bosch",
  "modelo": "F-0001",
  "preco": 45.0,
  "quantidadeEstoque": 10
}
```

Resposta `201 Created`:

```json
{
  "id": 1000,
  "nome": "Filtro de Oleo",
  "marca": "Bosch",
  "modelo": "F-0001",
  "preco": 45.0,
  "quantidadeEstoque": 10
}
```

### Ordens de servico

#### Operacoes CRUD e listagem

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/ordens-servico` | JWT | Criar ordem de servico |
| `GET` | `/ordens-servico?page=&pageSize=&clienteId=&status=&numero=&dataAberturaInicio=&dataAberturaFim=` | JWT | Listar ordens (paginado, com filtros) |
| `GET` | `/ordens-servico/{id}` | JWT | Obter ordem por ID |

#### Fluxo de status

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `PATCH` | `/ordens-servico/{id}/iniciar-diagnostico` | JWT | Recebida -> EmDiagnostico |
| `PATCH` | `/ordens-servico/{id}/finalizar-diagnostico` | JWT | EmDiagnostico -> AguardandoAprovacao |
| `PATCH` | `/ordens-servico/{id}/aprovar` | JWT | AguardandoAprovacao -> EmExecucao (ou AguardandoEstoque) |
| `PATCH` | `/ordens-servico/{id}/liberar-execucao` | JWT | AguardandoEstoque -> EmExecucao |
| `PATCH` | `/ordens-servico/{id}/finalizar` | JWT | EmExecucao -> Finalizada |
| `PATCH` | `/ordens-servico/{id}/registrar-pagamento` | JWT | Registrar pagamento (status Finalizada) |
| `PATCH` | `/ordens-servico/{id}/entregar` | JWT | Finalizada -> Entregue |
| `PATCH` | `/ordens-servico/{id}/cancelar` | JWT | Cancelar ordem |

#### Composicao do orcamento

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/ordens-servico/{id}/servicos` | JWT | Adicionar servico a ordem |
| `DELETE` | `/ordens-servico/{id}/servicos/{servicoId}` | JWT | Remover servico da ordem |
| `POST` | `/ordens-servico/{id}/pecas` | JWT | Adicionar peca a ordem |
| `DELETE` | `/ordens-servico/{id}/pecas/{pecaId}` | JWT | Remover peca da ordem |

#### Consultas e monitoramento

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `GET` | `/ordens-servico/{id}/historico` | JWT | Historico de eventos da ordem |
| `GET` | `/ordens-servico/{id}/notificacoes` | JWT | Notificacoes enviadas ao cliente |
| `GET` | `/ordens-servico/{id}/movimentacoes-estoque` | JWT | Movimentacoes de estoque agrupadas por peca |
| `GET` | `/ordens-servico/{id}/monitoramento` | JWT | Monitoramento individual da ordem |
| `GET` | `/ordens-servico/{id}/estimativa-tempo-servico` | JWT | Estimativa de tempo dos servicos |
| `GET` | `/ordens-servico/monitoramento?page=&pageSize=` | JWT | Resumo de monitoramento (paginado) |

#### Endpoints publicos da ordem

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/ordens-servico/{id}/ordem/resposta` | Publica (header `X-Tracking-Token`) | Cliente aprova ou recusa orcamento |

#### Criar ordem de servico

Request:

```json
{
  "clienteId": 1000,
  "veiculoId": 1000,
  "descricaoSolicitacao": "Cliente relatou ruido ao frear.",
  "observacoesRecepcao": "Problema ocorre em baixa velocidade.",
  "servicos": [
    { "servicoId": 1000 }
  ],
  "pecas": []
}
```

`servicos` e obrigatorio e deve conter ao menos um item. `pecas` e opcional e pode ser enviado como array vazio.

Exemplo com pecas informadas na abertura:

```json
{
  "clienteId": 1000,
  "veiculoId": 1000,
  "descricaoSolicitacao": "Troca de pneus dianteiros.",
  "observacoesRecepcao": "Cliente solicitou dois pneus da frente.",
  "servicos": [
    { "servicoId": 1000 }
  ],
  "pecas": [
    { "pecaId": 1000, "quantidade": 2 }
  ]
}
```

Resposta `201 Created`:

```json
{
  "id": 3001,
  "numero": "OS-20260710-3001",
  "codigoAcompanhamento": "abc123",
  "urlAcompanhamento": "/api/v1/acompanhamento-os/abc123",
  "tokenAcompanhamento": "token-unico-retornado-apenas-na-criacao",
  "clienteId": 1000,
  "veiculoId": 1000,
  "descricaoSolicitacao": "Cliente relatou ruido ao frear.",
  "observacoesRecepcao": "Problema ocorre em baixa velocidade.",
  "motivoCancelamento": null,
  "orcamentoEnviadoEm": null,
  "dataFinalizacao": null,
  "dataPagamento": null,
  "status": "Recebida",
  "dataAbertura": "2026-07-10T10:00:00",
  "dataConclusao": null,
  "valorTotal": 150.0,
  "servicos": [
    { "servicoId": 1000, "preco": 150.0, "tempoEstimado": 30 }
  ],
  "pecas": []
}
```

O campo `tokenAcompanhamento` so e retornado na criacao da ordem. Nas demais consultas ele e omitido.

#### Adicionar servico a ordem

Request:

```json
{
  "servicoId": 1000
}
```

#### Adicionar peca a ordem

Request:

```json
{
  "pecaId": 1000,
  "quantidade": 2
}
```

#### Cancelar ordem

Request:

```json
{
  "motivoCancelamento": "Cliente desistiu do reparo."
}
```

#### Responder ordem (endpoint publico)

```http
POST /ordens-servico/{id}/ordem/resposta
X-Tracking-Token: <token-de-acompanhamento>
```

Request para aprovar:

```json
{
  "aprovado": true
}
```

Request para recusar:

```json
{
  "aprovado": false,
  "motivoRecusa": "Orcamento acima do esperado."
}
```

#### Historico da ordem

Resposta `200 OK`:

```json
[
  {
    "id": 1,
    "ordemDeServicoId": 3001,
    "usuarioId": "1",
    "usuarioNome": "admin",
    "statusAnterior": null,
    "statusNovo": "Recebida",
    "tipoEvento": "OrdemCriada",
    "descricao": "Ordem de servico criada.",
    "dataEvento": "2026-07-10T10:00:00"
  }
]
```

#### Notificacoes da ordem

Resposta `200 OK`:

```json
[
  {
    "id": 1,
    "ordemDeServicoId": 3001,
    "canal": "Email",
    "tipoNotificacao": "LinkAcompanhamentoEnviado",
    "mensagem": "Link de acompanhamento da ordem OS-20260710-3001 enviado.",
    "recebida": false,
    "dataNotificacao": "2026-07-10T10:00:00"
  }
]
```

#### Monitoramento individual

Resposta `200 OK`:

```json
{
  "id": 3001,
  "numero": "OS-20260710-3001",
  "status": "EmExecucao",
  "dataAbertura": "2026-07-10T10:00:00",
  "dataFinalizacao": null,
  "estaFinalizada": false,
  "tempoDecorridoMinutos": 120,
  "tempoDecorridoHoras": 2.0,
  "tempoFinalizacaoMinutos": null,
  "tempoFinalizacaoHoras": null
}
```

#### Resumo de monitoramento

Resposta `200 OK`:

```json
{
  "totalOrdens": 15,
  "totalOrdensAbertas": 8,
  "totalOrdensFinalizadas": 7,
  "page": 1,
  "pageSize": 10,
  "totalPages": 2,
  "tempoMedioFinalizacaoMinutos": 180,
  "tempoMedioFinalizacaoHoras": 3.0,
  "ordens": [
    {
      "id": 3001,
      "numero": "OS-20260710-3001",
      "status": "EmExecucao",
      "dataAbertura": "2026-07-10T10:00:00",
      "dataFinalizacao": null,
      "estaFinalizada": false,
      "tempoDecorridoMinutos": 120,
      "tempoDecorridoHoras": 2.0,
      "tempoFinalizacaoMinutos": null,
      "tempoFinalizacaoHoras": null
    }
  ]
}
```

#### Estimativa de tempo

Resposta `200 OK`:

```json
{
  "ordemDeServicoId": 3001,
  "numero": "OS-20260710-3001",
  "status": "EmDiagnostico",
  "totalServicos": 2,
  "tempoEstimadoMinutos": 120,
  "tempoEstimadoHoras": 2.0,
  "servicos": [
    { "servicoId": 1000, "tempoEstimadoMinutos": 30, "tempoEstimadoHoras": 0.5 },
    { "servicoId": 1001, "tempoEstimadoMinutos": 90, "tempoEstimadoHoras": 1.5 }
  ]
}
```

#### Movimentacoes de estoque

Resposta `200 OK`:

```json
[
  {
    "pecaId": 1000,
    "nomePeca": "Filtro de Oleo",
    "marcaPeca": "Bosch",
    "modeloPeca": "F-0001",
    "quantidadeNaOrdem": 2,
    "totalMovimentacoes": 1,
    "movimentacoes": [
      {
        "id": 10,
        "pecaId": 1000,
        "ordemDeServicoId": 3001,
        "pedidoCompraId": null,
        "nomePeca": "Filtro de Oleo",
        "tipoMovimentacao": "BaixaParaOrdemDeServico",
        "quantidade": 2,
        "quantidadeAnterior": 10,
        "quantidadePosterior": 8,
        "descricao": "Baixa de estoque para a ordem de servico OS-20260710-3001.",
        "dataMovimentacao": "2026-07-10T10:00:00"
      }
    ]
  }
]
```

### Acompanhamento publico

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `GET` | `/acompanhamento-os/{codigo}` | Publica (header `X-Tracking-Token`) | Consultar status da OS pelo codigo publico |

```http
GET /acompanhamento-os/{codigo}
X-Tracking-Token: <token-de-acompanhamento>
```

Resposta `200 OK`:

```json
{
  "numero": "OS-20260710-3001",
  "codigoAcompanhamento": "abc123",
  "status": "EmExecucao",
  "dataAbertura": "2026-07-10T10:00:00",
  "dataUltimaAtualizacao": "2026-07-10T12:00:00",
  "orcamentoEnviadoEm": "2026-07-10T11:00:00",
  "dataFinalizacao": null,
  "dataPagamento": null,
  "dataConclusao": null
}
```

### Pedidos de compra

| Metodo | Rota | Autenticacao | Descricao |
|---|---|---|---|
| `POST` | `/pedidos-compra` | JWT | Criar pedido de compra manual |
| `GET` | `/pedidos-compra?page=&pageSize=` | JWT | Listar pedidos (paginado) |
| `GET` | `/pedidos-compra/ordem/{ordemDeServicoId}` | JWT | Listar pedidos por ordem de servico |
| `PATCH` | `/pedidos-compra/{id}/receber` | JWT | Registrar recebimento do pedido |

#### Criar pedido de compra

Request:

```json
{
  "ordemDeServicoId": 3001,
  "pecaId": 1000,
  "quantidadeSolicitada": 4,
  "observacao": "Pedido manual para reposicao"
}
```

Resposta `201 Created`:

```json
{
  "id": 1,
  "ordemDeServicoId": 3001,
  "pecaId": 1000,
  "nomePeca": "Filtro de Oleo",
  "marcaPeca": "Bosch",
  "modeloPeca": "F-0001",
  "quantidadeSolicitada": 4,
  "quantidadeRecebida": 0,
  "status": "Pendente",
  "dataSolicitacao": "2026-07-10T10:00:00",
  "dataRecebimento": null,
  "observacao": "Pedido manual para reposicao"
}
```

#### Registrar recebimento

Request:

```json
{
  "quantidadeRecebida": 4
}
```

---

## Fluxo da ordem de servico

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

## Regras de dominio relevantes

- A OS so pode iniciar diagnostico quando esta em `Recebida`.
- O diagnostico so pode ser finalizado se houver ao menos um servico e orcamento maior que zero.
- Servicos so podem ser adicionados e removidos em `EmDiagnostico`.
- Repetir o mesmo servico na OS retorna erro de negocio.
- Pecas so podem ser removidas em `EmDiagnostico`.
- Pecas podem ser adicionadas em `EmDiagnostico`, `AguardandoAprovacao` e `AguardandoEstoque`.
- A OS nao entra em `EmExecucao` sem validacao de estoque bem-sucedida.
- Uma OS em `AguardandoEstoque` so pode ir para `EmExecucao` pela rota `/liberar-execucao`.
- Pagamento so pode ser registrado apos finalizacao.
- Entrega so pode ocorrer apos pagamento.
- Cancelamento e permitido nos status: `Recebida`, `EmDiagnostico`, `AguardandoAprovacao` e `AguardandoEstoque`.

## Estoque e compras

Regra central:

- A transicao `AguardandoAprovacao -> EmExecucao` so ocorre apos validacao de estoque.
- Se houver disponibilidade total, o sistema baixa estoque e registra movimentacao.
- Se faltar item, a OS vai para `AguardandoEstoque` e o sistema pode gerar pedido de compra.
- `/aprovar` nao aceita OS em `AguardandoEstoque`.
- Apos o recebimento do pedido e reposicao do estoque, use `PATCH /ordens-servico/{id}/liberar-execucao`.

Rastreabilidade:

- `GET /ordens-servico/{id}/historico`
- `GET /ordens-servico/{id}/movimentacoes-estoque` (agrupado por peca)
- `GET /pedidos-compra/ordem/{ordemDeServicoId}`
- `GET /pedidos-compra?page=&pageSize=`
