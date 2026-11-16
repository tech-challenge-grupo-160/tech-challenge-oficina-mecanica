# language: pt
@saga @fluxo-feliz
Funcionalidade: Fluxo completo da ordem de serviço
  Como oficina mecânica
  Quero que a OS passe por Execução e Billing coordenada pelo OS Service
  Para que cada etapa aconteça na ordem certa, do diagnóstico à entrega

  O OS Service orquestra a Saga (ADR-0004). Ele envia comandos para a Execução
  e o Billing pelo outbox e reage aos eventos que esses serviços publicam.
  Nos cenários, os eventos chegam aos consumidores reais do OS Service com o
  mesmo contrato que Billing e Execução publicam no SNS.

  Cenário: Da abertura à entrega do veículo, passando pelos três serviços
    Dado uma OS aberta para o veículo do cliente com 1 serviço e 2 peças
    Quando a recepção solicita o diagnóstico
    Então o OS Service envia o comando IniciarDiagnosticoMessage para a Execução
    E a OS fica "Recebida"
    Quando a Execução informa que o diagnóstico começou
    Então a OS fica "EmDiagnostico"
    Quando a Execução conclui o diagnóstico com 1 serviço de R$ 150 e 2 peças de R$ 45
    Então a OS fica "AguardandoOrcamento"
    E o OS Service envia o comando GerarOrcamentoMessage para o Billing
    Quando o Billing gera o orçamento
    Então a OS fica "AguardandoAprovacao"
    E a saga fica "AguardandoAprovacao"
    Quando o cliente aprova o orçamento no Billing
    Então o OS Service envia o comando ReservarPecasMessage para a Execução
    E a saga fica "ReservandoPecas"
    Quando a Execução reserva todas as peças
    Então a OS fica "EmExecucao"
    E o OS Service envia o comando IniciarExecucaoMessage para a Execução
    Quando a Execução finaliza o reparo
    Então a OS fica "Finalizada"
    E o OS Service envia o comando SolicitarPagamentoMessage para o Billing
    E a saga fica "AguardandoPagamento"
    Quando o Billing confirma o pagamento
    Então a saga fica "AguardandoEntrega"
    Quando a recepção entrega o veículo
    Então a OS fica "Entregue"
    E o histórico da OS registra os eventos:
      | evento                 |
      | OrdemCriada            |
      | DiagnosticoIniciado    |
      | DiagnosticoFinalizado  |
      | OrcamentoGerado        |
      | ReservaPecasSolicitada |
      | PecasReservadas        |
      | ServicoFinalizado      |
      | PagamentoRegistrado    |
      | VeiculoEntregue        |

  Cenário: Evento repetido pela mensageria não repete o passo da Saga
    Dado uma OS com orçamento de R$ 240 aguardando aprovação do cliente
    Quando o Billing entrega a mesma aprovação do orçamento duas vezes
    Então o OS Service envia o comando ReservarPecasMessage para a Execução uma única vez
    E a saga fica "ReservandoPecas"
