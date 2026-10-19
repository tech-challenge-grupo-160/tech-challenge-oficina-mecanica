# Arquitetura da Fase 4 — microsserviços

> **Status:** desenho aceito na Sprint 1 (ADRs [0003](../../adrs/0003-divisao-em-microsservicos.md) a [0006](../../adrs/0006-banco-por-servico.md), 2026-10-17). Ainda não implementado por inteiro: mensageria, bancos novos e Saga entram nas Sprints 2 a 4. É a base do diagrama geral da entrega ([F4-40](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/171)).

## Visão geral

```mermaid
flowchart TB
    usuarios(["Cliente e funcionários"])
    mp["Mercado Pago"]
    dd["Datadog<br/>APM, logs e métricas"]

    subgraph aws["AWS"]
        gw["API Gateway<br/>authorizer da Lambda"]
        lambda["Lambda de autenticação<br/>CPF → JWT"]

        subgraph eks["EKS · namespace oficina-mecanica"]
            os["OS Service<br/>OS, cliente, veículo<br/>orquestra a Saga"]
            billing["Billing<br/>orçamento e pagamento"]
            exec["Execução<br/>diagnóstico, peças e estoque"]
        end

        bus{{"SNS + SQS<br/>eventos, comandos e DLQ"}}

        osdb[("PostgreSQL<br/>RDS atual")]
        billdb[("DynamoDB<br/>tabela única")]
        execdb[("PostgreSQL<br/>RDS próprio")]
    end

    usuarios --> gw
    gw -.-> lambda
    gw --> os
    gw --> billing
    gw --> exec

    os --- osdb
    billing --- billdb
    exec --- execdb

    os <--> bus
    billing <--> bus
    exec <--> bus

    billing <--> mp
    eks -.-> dd
```

| Serviço | Repositório | Banco | Responsabilidade |
|---|---|---|---|
| OS Service | [tech-challenge-oficina-mecanica](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica) | PostgreSQL (RDS atual) | Cliente, veículo, OS e seu status, histórico, acompanhamento público, notificações, login de funcionários e orquestração da Saga |
| Billing | [tech-challenge-oficina-mecanica-billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing) | DynamoDB | Orçamento (gerar, enviar, aprovar, recusar) e pagamento via Mercado Pago |
| Execução | [tech-challenge-oficina-mecanica-execucao](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao) | PostgreSQL (RDS próprio) | Diagnóstico e reparo, fila de execução, catálogo de serviços e peças, estoque e pedidos de compra |

Regras que o desenho assume:

- **Nenhum serviço acessa o banco de outro.** Dado de outro serviço chega por evento, com cópia mínima local. O CPF não sai do OS Service.
- **Só o OS Service altera o status da OS**, sempre em reação a eventos.
- **REST síncrono entre serviços** só na validação do catálogo na abertura da OS (OS Service → Execução, pelo DNS do cluster).
- **Mensageria com outbox e consumo idempotente**; toda fila tem DLQ ([ADR-0005](../../adrs/0005-mensageria-sns-sqs.md)).

## Caminho feliz da Saga

Saga orquestrada pelo OS Service ([ADR-0004](../../adrs/0004-saga-orquestrada.md)). O pagamento continua no fim, depois da execução.

```mermaid
sequenceDiagram
    autonumber
    participant OS as OS Service
    participant EX as Execução
    participant BI as Billing

    Note over OS: OS aberta
    OS->>EX: solicitar diagnóstico
    EX-->>OS: diagnóstico concluído (itens com preço)
    OS->>BI: gerar orçamento
    BI-->>OS: orçamento enviado
    Note over BI: cliente aprova
    BI-->>OS: orçamento aprovado
    OS->>EX: reservar peças
    EX-->>OS: peças reservadas
    Note over EX: reparo
    EX-->>OS: execução finalizada
    OS->>BI: cobrar
    BI-->>OS: pagamento confirmado
    Note over OS: entrega liberada
```

Compensações, quando um passo falha:

| Falha | O que é desfeito |
|---|---|
| Orçamento recusado ou expirado (7 dias) | Billing cancela o orçamento; OS cancelada |
| Falta de estoque | Pedido de compra gerado e OS aguardando estoque; se cancelada, Execução libera as reservas |
| Execução abortada | Execução devolve as peças ao estoque; Billing cancela o orçamento; OS cancelada |
| Pagamento recusado | Nada é desfeito (o conserto já foi feito): a OS aguarda pagamento e a cobrança é repetida |
