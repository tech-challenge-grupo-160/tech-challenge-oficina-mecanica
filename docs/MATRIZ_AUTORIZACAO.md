# Matriz de autorização — OS Service

Última atualização: 2026-11-08

Esta matriz representa somente a superfície HTTP atual do OS Service na Fase 4.

## Níveis

| Nível | Exigência |
|---|---|
| Anônimo | Não exige token |
| Autenticado | Exige JWT válido |
| Cliente | Exige JWT válido com a policy `Cliente` |

## Rotas

| Recurso | Métodos e rotas | Nível |
|---|---|---|
| Autenticação local | `POST /api/v1/auth/login` | Anônimo |
| Acompanhamento | `GET /api/v1/acompanhamento-os/{codigo}` | Cliente |
| Clientes | Todas as rotas de `/api/v1/clientes` | Autenticado |
| Veículos | Todas as rotas de `/api/v1/veiculos` | Autenticado |
| Ordens de serviço | Todas as rotas de `/api/v1/ordens-servico` | Autenticado |

## Limites de contexto

Não existem mais controllers de catálogo, estoque, pedidos de compra, orçamento, diagnóstico, reparo ou pagamento neste serviço. A autorização dessas operações deve ser definida nos microsserviços proprietários:

- **Execução:** catálogo, diagnóstico, peças, estoque e reparo;
- **Billing:** orçamento e pagamento;
- **OS Service:** clientes, veículos, OS, histórico, notificações e orquestração.

## Observação sobre autenticação

O `AuthController` permanece para o fluxo local de funcionários. A autenticação externa de clientes por CPF e a validação na borda permanecem na Lambda/API Gateway; este corte não altera essa decisão.
