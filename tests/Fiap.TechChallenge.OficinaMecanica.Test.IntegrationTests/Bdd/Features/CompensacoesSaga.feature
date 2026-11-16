# language: pt
@saga @compensacao
Funcionalidade: Compensações da Saga da ordem de serviço
  Como oficina mecânica
  Quero que uma falha em qualquer serviço seja tratada pelo OS Service
  Para que a OS nunca fique num estado inconsistente

  Até a execução os passos são compensáveis; a execução é o ponto sem volta;
  o pagamento, depois dela, é repetível (ADR-0004).

  Cenário: Orçamento recusado cancela a OS sem reservar peças
    Dado uma OS com orçamento de R$ 240 aguardando aprovação do cliente
    Quando o cliente recusa o orçamento no Billing
    Então a OS fica "Cancelada"
    E a saga fica "Cancelada"
    E o OS Service não envia o comando ReservarPecasMessage
    E o histórico da OS registra os eventos:
      | evento            |
      | OrcamentoRecusado |

  Cenário: Falta de estoque segura a OS até a reposição das peças
    Dado uma OS com orçamento de R$ 240 aguardando aprovação do cliente
    E o cliente aprova o orçamento no Billing
    Quando a Execução informa estoque insuficiente para 1 das 2 peças
    Então a OS fica "AguardandoEstoque"
    E a saga fica "AguardandoEstoque"
    E o OS Service não envia o comando IniciarExecucaoMessage
    Quando a Execução repõe o estoque e reserva todas as peças
    Então a OS fica "EmExecucao"
    E o OS Service envia o comando IniciarExecucaoMessage para a Execução

  Cenário: Pagamento recusado não desfaz o reparo e permite nova cobrança
    Dado uma OS finalizada aguardando pagamento
    Quando o Billing recusa o pagamento com o motivo "cancelled: expired"
    Então a OS fica "Finalizada"
    E a saga fica "PagamentoRecusado"
    E o histórico da OS registra os eventos:
      | evento            |
      | PagamentoRecusado |
    Quando a recepção entrega o veículo
    Então a entrega é recusada
    Quando a recepção pede uma nova cobrança
    Então a nova cobrança é aceita
    E o OS Service pede ao Billing a tentativa de cobrança 2
    E a saga fica "AguardandoPagamento"
    Quando o Billing confirma o pagamento
    E a recepção entrega o veículo
    Então a OS fica "Entregue"

  Cenário: Nova cobrança só depois de um pagamento recusado
    Dado uma OS finalizada aguardando pagamento
    Quando a recepção pede uma nova cobrança
    Então a nova cobrança é recusada
    E a saga fica "AguardandoPagamento"
