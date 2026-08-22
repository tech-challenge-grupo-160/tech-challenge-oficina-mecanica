# Postman — Collection e Environment

Este diretorio contem a collection e o environment do Postman para testar a API da Oficina Mecanica.

## Arquivos

| Arquivo | Descricao |
|---|---|
| `TechChallenge OficinaMecanica.postman_collection.json` | Collection com todos os endpoints organizados por recurso |
| `TechChallenge OficinaMecanica.postman_environment.json` | Variaveis de ambiente (baseUrl, token, IDs) |

## Como importar

1. Abra o Postman.
2. Clique em **Import** (canto superior esquerdo).
3. Arraste ou selecione os dois arquivos `.json` deste diretorio.
4. A collection **TechChallenge OficinaMecanica** e o environment de mesmo nome aparecerao no painel lateral.
5. Selecione o environment **TechChallenge OficinaMecanica** no seletor de ambientes (canto superior direito).

## Variaveis de ambiente

| Variavel | Valor padrao | Descricao |
|---|---|---|
| `baseUrl` | `http://localhost:8080` | URL base da API |
| `token` | *(vazio)* | JWT obtido no login — preencher manualmente apos autenticar |
| `cpfCnpj` | `47654866801` | CPF/CNPJ usado nos requests de cliente |
| `clienteId` | `1000` | ID do cliente criado pelo seed |
| `veiculoId` | `1000` | ID do veiculo criado pelo seed |
| `placa` | `ABC1234` | Placa usada nos requests de veiculo |
| `ordemServicoId` | `3004` | ID da OS para testes de fluxo |
| `ordemServicoNumero` | `OS-20260418-3004` | Numero da OS |
| `codigoAcompanhamento` | *(vazio)* | Codigo de acompanhamento retornado na criacao da OS |
| `servicoId` | `1002` | ID do servico |
| `pecaId` | `1004` | ID da peca |
| `pedidoCompraId` | `1` | ID do pedido de compra |

Ajuste os valores conforme o estado do seu banco. Os IDs padrao correspondem aos dados do seed de desenvolvimento.

## Configurar autenticacao

A collection usa a variavel `{{token}}` no header `Authorization: Bearer {{token}}`. Para configurar:

1. Execute o request **Auth > Login** (usuario `admin`, senha `admin123`).
2. Copie o valor do campo `token` da resposta.
3. No environment, cole o valor na variavel `token`.
4. Todos os demais requests usarao o token automaticamente.

## Estrutura da collection

A collection esta organizada por recurso, na ordem recomendada para testar o fluxo completo:

### 1. Auth

- `POST Login` — obter JWT

### 2. Clientes

- Criar, consultar por documento, listar paginado, atualizar, excluir

### 3. Veiculos

- Criar veiculo para cliente, listar por cliente, listar todos, consultar por placa, editar, excluir

### 4. Servicos

- Criar, listar, consultar por ID, atualizar, excluir

### 5. Pecas

- Criar, listar, consultar por ID, atualizar, excluir

### 6. Ordens de Servico

Fluxo completo da OS na ordem operacional:

1. `POST Criar OS` — abre a ordem com servicos e pecas
2. `GET Obter OS por Id` — consulta a OS criada
3. `PATCH Iniciar Diagnostico` — Recebida → EmDiagnostico
4. `POST Adicionar Servico na OS` — monta orcamento
5. `POST Adicionar Peca na OS` — monta orcamento
6. `PATCH Finalizar Diagnostico` — EmDiagnostico → AguardandoAprovacao
7. `PATCH Aprovar Orcamento` — AguardandoAprovacao → EmExecucao (ou AguardandoEstoque)
8. `PATCH Liberar Execucao OS` — AguardandoEstoque → EmExecucao (se bloqueada)
9. `PATCH Finalizar OS` — EmExecucao → Finalizada
10. `PATCH Registrar Pagamento OS` — registra pagamento
11. `PATCH Entregar OS` — Finalizada → Entregue

Consultas auxiliares:

- `GET Listar OS Paginado` — com filtros por cliente, status, numero, periodo
- `GET Historico OS` — eventos da ordem
- `GET Monitoramento OS` / `GET Monitoramento OS por id` — tempo decorrido e metricas
- `GET Estimativa Tempo Servico por OS` — soma dos tempos estimados
- `GET Movimentacao Estoque OS por id` — movimentacoes agrupadas por peca
- `GET Acompanhamento OS` — consulta do cliente autenticado com JWT da Lambda
- `POST Aprovacao de orcamento` — resposta do cliente autenticado com JWT da Lambda
- `PATCH Cancelar OS` — cancelamento com motivo

### 7. Pedidos de Compra

- Criar pedido manual, listar todos, listar por OS, registrar recebimento

## Fluxo recomendado para teste completo

```text
1. Login (obter token)
2. Criar Cliente
3. Criar Veiculo para Cliente
4. Criar Servico
5. Criar Peca
6. Criar OS (com servico e peca)
7. Iniciar Diagnostico
8. Adicionar Servico / Peca (opcional)
9. Finalizar Diagnostico
10. Aprovar Orcamento
11. Finalizar OS
12. Registrar Pagamento
13. Entregar OS
```

Para testar o fluxo de falta de estoque, crie uma peca com `quantidadeEstoque: 0` e adicione-a a OS antes de aprovar. A OS ira para `AguardandoEstoque`. Crie e receba um pedido de compra, depois use `Liberar Execucao OS`.
