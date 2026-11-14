# Sequência da Saga (Fase 4)

Saga orquestrada pelo OS Service ([ADR-0004](../../adrs/0004-saga-orquestrada.md)).
Os nomes são os das mensagens reais ([catálogo](../../mensageria/catalogo.md)).
Comandos (→) vão direto à fila do serviço; eventos (⇢) vão pelo tópico SNS do
produtor até a fila do OS. Todos os passos são cobertos pelo
[BDD](../../../README.md#bdd-do-fluxo-completo-da-os-f4-36).

Para o PDF, exporte cada diagrama como descrito em
[Exportar para imagem](../README.md#exportar-para-imagem).

## Caminho feliz

```mermaid
sequenceDiagram
    autonumber
    actor R as Recepcao
    participant OS as OS Service (orquestrador)
    participant EX as Execucao
    participant BI as Billing
    actor C as Cliente

    R->>OS: POST /ordens-servico
    OS->>EX: consulta catalogo (HTTP)
    Note over OS: OS Recebida
    R->>OS: PATCH /iniciar-diagnostico
    OS->>EX: IniciarDiagnosticoMessage
    EX-->>OS: DiagnosticoIniciadoMessage
    Note over OS: OS EmDiagnostico
    EX-->>OS: DiagnosticoConcluidoMessage
    Note over OS: OS AguardandoOrcamento
    OS->>BI: GerarOrcamentoMessage
    BI-->>OS: OrcamentoGeradoMessage
    Note over OS: OS e saga AguardandoAprovacao
    C->>BI: PATCH /orcamentos/.../aprovar
    BI-->>OS: OrcamentoAprovadoMessage
    OS->>EX: ReservarPecasMessage
    EX-->>OS: PecasReservadasMessage
    Note over OS: OS EmExecucao
    OS->>EX: IniciarExecucaoMessage
    EX-->>OS: ExecucaoFinalizadaMessage
    Note over OS: OS Finalizada, saga AguardandoPagamento
    OS->>BI: SolicitarPagamentoMessage (tentativa 1)
    C->>BI: paga no Checkout Pro
    BI-->>OS: PagamentoConfirmadoMessage
    Note over OS: saga AguardandoEntrega
    R->>OS: PATCH /entregar
    Note over OS: OS Entregue
```

## Compensações

```mermaid
sequenceDiagram
    autonumber
    actor R as Recepcao
    participant OS as OS Service (orquestrador)
    participant EX as Execucao
    participant BI as Billing
    actor C as Cliente

    rect rgba(220, 80, 80, 0.12)
    Note over OS,C: Orcamento recusado (passo compensavel)
    C->>BI: PATCH /orcamentos/.../recusar
    BI-->>OS: OrcamentoRecusadoMessage
    Note over OS: OS e saga Cancelada<br/>nenhuma peca reservada
    end

    rect rgba(230, 160, 40, 0.14)
    Note over OS,C: Falta de estoque (espera e retoma)
    BI-->>OS: OrcamentoAprovadoMessage
    OS->>EX: ReservarPecasMessage
    EX-->>OS: EstoqueInsuficienteMessage
    Note over OS: OS e saga AguardandoEstoque
    Note over EX: POST /estoque/pecas/{id}/reposicoes
    EX-->>OS: PecasReservadasMessage
    OS->>EX: IniciarExecucaoMessage
    end

    rect rgba(70, 130, 220, 0.12)
    Note over OS,C: Pagamento recusado (depois do ponto sem volta: repetivel)
    OS->>BI: SolicitarPagamentoMessage (tentativa 1)
    BI-->>OS: PagamentoRecusadoMessage (tentativa 1)
    Note over OS: OS segue Finalizada, saga PagamentoRecusado<br/>entrega bloqueada, reparo NAO desfeito
    R->>OS: POST /pagamento/nova-tentativa
    OS->>BI: SolicitarPagamentoMessage (tentativa 2)
    BI-->>OS: PagamentoConfirmadoMessage (tentativa 2)
    Note over OS: entrega liberada
    end
```

| Falha | Tipo de passo | Tratamento |
|---|---|---|
| Orçamento recusado | Compensável | OS e saga `Cancelada`; nenhum comando de reserva é enviado |
| Falta de estoque | Compensável (espera) | OS em `AguardandoEstoque`; a reposição completa a reserva e a saga retoma |
| Pagamento recusado | Repetível (depois do *pivot*) | O reparo não é desfeito; a OS segue `Finalizada` sem pagamento e uma nova tentativa gera outro pagamento no Billing, sem duplicar cobrança |
| Mensagem repetida | — | Inbox por `MessageId`: o passo não se repete |

Fora do escopo desta fase: expiração do orçamento por tempo e falha da execução
com devolução de peças (decisão de 13/11/2026 no board).
