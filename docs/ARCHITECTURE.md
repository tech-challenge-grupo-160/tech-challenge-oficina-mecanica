# Arquitetura do OS Service

Última atualização: 2026-11-08

Este repositório contém o **OS Service**, dono do agregado de ordem de serviço e
orquestrador da Saga da oficina. Ele deixou de ser o monólito da Fase 3: catálogo,
diagnóstico, estoque, reparo, orçamento e pagamento foram separados em serviços
independentes.

## Limites dos microsserviços

| Serviço | Responsabilidades | Persistência |
|---|---|---|
| OS Service | Clientes, veículos, OS, estado global, histórico, notificações e orquestração | PostgreSQL próprio |
| Execução | Catálogo, diagnóstico, peças, estoque, reserva e reparo | PostgreSQL próprio |
| Billing | Orçamento e pagamento | DynamoDB próprio |

Nenhum serviço acessa diretamente o banco de outro. Identificadores externos de
serviços e peças aparecem na OS somente como parte de snapshots comerciais.

## Comunicação

São usados dois estilos de comunicação, conforme a necessidade:

- **HTTP síncrono:** operações que precisam de resposta imediata, como a consulta
  em lote ao catálogo da Execução durante a abertura da OS;
- **SNS + SQS assíncrono:** comandos e eventos da Saga, com LocalStack no ambiente
  local, DLQs, Inbox para idempotência de consumo e Outbox para publicação
  confiável.

Uma resposta `202 Accepted` indica que o comando foi persistido e será processado
assincronamente; ela não afirma que o microsserviço de destino já concluiu a ação.

## Fluxo principal da Saga

```text
OS aberta
  -> solicitar diagnóstico
  -> diagnóstico iniciado
  -> diagnóstico concluído
  -> gerar orçamento
  -> orçamento gerado
  -> cliente aprova ou recusa
  -> reservar peças (quando houver)
  -> iniciar e concluir reparo
  -> gerar e confirmar pagamento
  -> entregar veículo
```

O OS Service é o único serviço que altera o estado global da OS. Execução e
Billing alteram seus próprios agregados e publicam os resultados; o OS Service
consome esses eventos e aplica a transição correspondente.

### Estados da OS

```text
Recebida
  -> EmDiagnostico
  -> AguardandoOrcamento
  -> AguardandoAprovacao
  -> EmExecucao
  -> Finalizada
  -> Entregue
```

Também existem os caminhos de `Cancelada` e `AguardandoEstoque`. A falta de
estoque é uma resposta explícita da Execução e não deixa a Saga silenciosamente
travada.

## Abertura e snapshots

A abertura preserva o contrato funcional da oficina:

- exige ao menos um serviço;
- aceita peças opcionais e suas quantidades;
- valida cliente e veículo no banco do OS Service;
- consulta serviços e peças na Execução pelo `ICatalogoExecucao`;
- persiste na OS o ID externo, preço, tempo estimado e quantidade aplicáveis
  naquele momento.

As tabelas de itens da OS não possuem FK para um catálogo local. Dessa forma,
uma alteração futura de preço ou descrição na Execução não altera retroativamente
uma OS já aberta.

## Persistência do OS Service

O PostgreSQL deste serviço mantém somente dados pertencentes ao seu contexto:

- `Cliente`;
- `Veiculo`;
- `OrdemServico`;
- `OrdemServicoItemServico`;
- `OrdemServicoItemPeca`;
- `OrdemServicoHistorico`;
- `NotificacaoCliente`;
- `Usuario`;
- `SagaInicioDiagnostico`;
- `InboxMessage`;
- `OutboxMessage`.

As tabelas locais `Servico`, `Peca`, `MovimentacaoEstoque` e `PedidoCompra` foram
removidas. Seus dados operacionais pertencem à Execução.

## Organização do código

```text
src/
  API/              Controllers, contratos HTTP, mappers e bootstrap
  Application/      Commands, queries, handlers, validators e portas
  Domain/           Agregado da OS, clientes, veículos e regras de domínio
  Infrastructure/   EF Core, PostgreSQL, HTTP resiliente e mensageria AWS
  Shared/           Utilitários compartilhados e logging
```

As dependências apontam para dentro:

```text
API -> Application -> Domain
API -> Infrastructure (somente composition root)
Infrastructure -> Application + Domain
```

Controllers não acessam `DbContext` ou repositórios concretos. Casos de uso são
despachados por MediatR, regras permanecem no domínio e adaptadores externos são
registrados pela Infrastructure.

## Consistência e resiliência

- cada microsserviço confirma apenas sua própria transação local;
- comandos e eventos carregam `messageId` e `correlationId`;
- Inbox impede o reprocessamento do mesmo evento;
- Outbox evita a perda da mensagem entre persistência e publicação;
- consumidores toleram redelivery e aplicam transições idempotentes;
- retries e circuit breaker protegem a consulta HTTP ao catálogo;
- mensagens não processáveis seguem para DLQ;
- histórico da OS registra as mudanças relevantes da Saga.

Não existe transação ACID distribuída. A consistência entre serviços é eventual e
as falhas de negócio são representadas por estados ou eventos explícitos.

## Autenticação

O `AuthController` permanece para autenticação local de funcionários. Na AWS, a
autenticação externa de clientes por CPF e a validação na borda permanecem na
Lambda e no API Gateway. Essa decisão é independente da divisão dos domínios.

## Evolução do banco

O schema é versionado por migrations do Entity Framework Core. A migration
`RemoveLegacyCatalogStockAndPurchasing` remove as tabelas e FKs do antigo
catálogo/estoque local, preservando os itens já registrados nas ordens como
snapshots. O `Down` recria registros mínimos para esses IDs antes de restaurar as
FKs, permitindo rollback técnico sem invalidar ordens existentes.

Para detalhes das rotas atuais, consulte [API_REFERENCE.md](API_REFERENCE.md).
As decisões históricas e seus contextos permanecem registradas em [adrs/](adrs/).
