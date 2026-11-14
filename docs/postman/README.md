# Postman — fluxo completo da OS (Fase 4)

Collection e environments para exercitar o fluxo da ordem de serviço pelos três
microsserviços (OS Service, Billing e Execução), incluindo as compensações da
Saga.

## Arquivos

| Arquivo | Conteúdo |
|---|---|
| `oficina-mecanica-fase-4.postman_collection.json` | Collection com o fluxo em pastas numeradas, da autenticação à entrega |
| `oficina-mecanica-local.postman_environment.json` | Serviços locais: OS `:8080`, Billing `:8081`, Execução `:8082` |
| `oficina-mecanica-dev.postman_environment.json` | Ambiente dev na AWS, tudo pelo API Gateway |
| `oficina-mecanica-hom.postman_environment.json` | Ambiente de homologação na AWS |
| `oficina-mecanica-prod.postman_environment.json` | Ambiente de produção na AWS |

## Como usar

1. Importe a collection e os environments no Postman (**Import**, arraste os
   arquivos).
2. Selecione o environment.
3. Nos environments da AWS, preencha `gatewayUrl` com a URL do API Gateway do
   ambiente. Ela é o output `gateway_url` do Terraform do
   [infra-k8s](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s):

   ```bash
   terraform -chdir=infra output -raw gateway_url
   ```

4. Preencha `senha` com a senha do usuário `admin` do seed (a mesma do README
   principal). Ela fica marcada como secreta e não vai para o arquivo exportado.
5. Rode as pastas em ordem. Cada requisição guarda no environment os ids que a
   próxima usa: `token`, `ordemServicoId`, `orcamentoId`, `pagamentoId` e
   `codigoAcompanhamento`.

Os passos entre serviços são **assíncronos** (SNS/SQS). Quando o status da OS
ainda não mudou, aguarde alguns segundos e repita a consulta.

## O fluxo

| Pasta | O que mostra |
|---|---|
| 00 - Autenticação | JWT do funcionário (`POST /api/v1/auth/login`) e do cliente pelo CPF na Lambda (`POST /auth`, só na AWS) |
| 01 - Cadastro e abertura da OS | Cliente e veículo (opcional), consulta ao catálogo da Execução e abertura da OS |
| 02 - Diagnóstico | O OS manda o comando; na Execução, resultado e conclusão do diagnóstico |
| 03 - Orçamento | Orçamento gerado pelo Billing; aprovação, ou recusa como compensação (a OS é cancelada) |
| 04 - Reserva de peças e reparo | Reserva, reposição de estoque (retoma a saga se faltou peça) e finalização do reparo |
| 05 - Pagamento | Link do Checkout Pro; na máquina local, confirmação ou recusa simulada; nova cobrança depois da recusa |
| 06 - Entrega e acompanhamento | Entrega (bloqueada sem pagamento), histórico, monitoramento e acompanhamento pelo cliente |
| 07 - Consultas e cadastros | Listagens de apoio |

Na AWS, o pagamento é feito no Checkout Pro do Mercado Pago, com cartão de
teste ([MERCADO-PAGO.md do Billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing/blob/develop/docs/MERCADO-PAGO.md)).
Os endpoints `simular-*` respondem 404 fora de Development/Testing.

## Autenticação

A collection usa `Bearer {{token}}` em todas as requisições, exceto no login e
no `/auth`. Na AWS, o API Gateway valida o token no authorizer Lambda antes de
encaminhar para o ALB, que escolhe o serviço pelo caminho
(`/api/v1/orcamentos*` e `/api/v1/pagamentos*` vão para o Billing;
`/api/v1/catalogo*`, `/api/v1/diagnosticos*`, `/api/v1/estoque*` e
`/api/v1/execucoes*` vão para a Execução; o resto vai para o OS Service).

Para ver o acompanhamento como o cliente, troque o token da requisição por
`{{tokenCliente}}` na aba *Authorization*.

## Contratos

Os contratos OpenAPI de cada serviço estão versionados:
[OS Service](../openapi/os-service.json),
[Billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing/blob/develop/docs/openapi/billing.json) e
[Execução](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao/blob/develop/docs/openapi/execucao.json).
