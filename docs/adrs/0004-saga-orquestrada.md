# ADR-0004: Coordenar o fluxo da OS com uma Saga orquestrada no OS Service

| | |
|---|---|
| **Status** | Proposta |
| **Data** | 2026-10-12 |
| **Decisores** | Grupo 160 |
| **Issue** | [#133](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/133) |

## Contexto

Com a divisão em três serviços ([ADR-0003](0003-divisao-em-microsservicos.md)), o atendimento de uma OS passa a atravessar três bancos diferentes. O que hoje é uma transação única, como aprovar o orçamento, conferir o estoque, dar baixa nas peças e mudar o status, deixa de poder ser feito com um `commit` só.

O enunciado exige o **Saga Pattern** para coordenar esse fluxo, com **rollback e compensação em caso de falha em qualquer etapa**, e pede que a escolha entre orquestração e coreografia seja **documentada e justificada no README**. O vídeo de entrega precisa mostrar a Saga funcionando e o tratamento de falhas.

Uma Saga é uma sequência de transações locais, uma por serviço. Se um passo falha, os passos anteriores são desfeitos por **transações de compensação** (por exemplo, devolver ao estoque as peças reservadas), já que não existe rollback distribuído.

## Decisão

Implementar uma **Saga orquestrada**, com o orquestrador **dentro do OS Service**: uma máquina de estados por OS, persistida no banco do OS Service, que envia comandos ao Billing e à Execução e reage aos eventos que eles publicam.

### Por que orquestrada

- **O OS Service já é o único serviço que altera o status da OS** (ADR-0003). O orquestrador é a evolução natural desse papel, não um componente novo.
- **As compensações ficam num lugar só.** Para saber o que desfazer quando algo falha, basta olhar a máquina de estados, não seguir eventos espalhados por três repositórios.
- **O estado de cada OS é consultável** ([F4-30](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/161)), o que ajuda na operação, no BDD e no vídeo.
- **Billing e Execução não se conhecem.** Cada um conversa só com o orquestrador, o que evita dependências cruzadas entre eles.

### Os passos

| # | Passo | Quem executa | Mensagem que inicia | Sucesso | Falha |
|---|---|---|---|---|---|
| 1 | Abrir a OS | OS Service | requisição HTTP | `os.aberta` | — |
| 2 | Diagnosticar | Execução | comando `execucao.solicitar-diagnostico` | `execucao.diagnostico-concluido` (itens com preço) | OS cancelada antes do diagnóstico |
| 3 | Gerar e enviar orçamento | Billing | comando `billing.gerar-orcamento` | `billing.orcamento-enviado` | — |
| 4 | Aprovação do cliente | Billing | ação do cliente | `billing.orcamento-aprovado` | `billing.orcamento-recusado` ou expiração |
| 5 | Reservar peças | Execução | comando `execucao.reservar-pecas` | `execucao.pecas-reservadas` | `execucao.estoque-insuficiente` (aguarda pedido de compra) |
| 6 | Executar o reparo | Execução | reserva concluída | `execucao.finalizada` | `execucao.abortada` |
| 7 | Cobrar | Billing | comando `billing.cobrar` | `billing.pagamento-confirmado` | `billing.pagamento-recusado` |
| 8 | Entregar | OS Service | requisição HTTP | `os.entregue` | — |

Os nomes das mensagens são ilustrativos; o catálogo oficial é definido na [ADR-0005](0005-mensageria-sns-sqs.md).

### Estados da Saga e status da OS

```
Diagnostico ──► AguardandoAprovacao ──► ReservandoPecas ──► EmExecucao ──► AguardandoPagamento ──► Concluida
     │                  │                  │      ▲              │
     │                  │                  ▼      │              │
     │                  │           AguardandoEstoque            │
     ▼                  ▼                  ▼                     ▼
  Cancelada  ◄──── Cancelada  ◄──────  Cancelada  ◄────────  Cancelada
```

| Estado da Saga | Status da OS (enum atual) |
|---|---|
| Diagnostico | `Recebida`, `EmDiagnostico` |
| AguardandoAprovacao | `AguardandoAprovacao` |
| ReservandoPecas, AguardandoEstoque | `AguardandoEstoque` (só se faltar peça) |
| EmExecucao | `EmExecucao` |
| AguardandoPagamento | `Finalizada` |
| Concluida | `Entregue` |
| Cancelada | `Cancelada` |

O enum `StatusOrdemDeServico` e a regra de transições em `OrdemDeServico.ValidarTransicaoDeStatus` continuam valendo: o orquestrador só pede transições que o domínio já aceita.

### Compensações

| Falha | Em que estado | Compensação |
|---|---|---|
| Orçamento recusado ou expirado | AguardandoAprovacao | Billing cancela o orçamento; OS `Cancelada` com motivo |
| Cancelamento enquanto aguarda estoque | AguardandoEstoque | Execução libera as reservas parciais e cancela os pedidos de compra pendentes; Billing cancela o orçamento; OS `Cancelada` |
| Execução abortada | EmExecucao | Execução devolve ao estoque as peças não usadas; Billing cancela o orçamento; OS `Cancelada` |
| Pagamento recusado | AguardandoPagamento | **Não desfaz a execução** (o carro já foi consertado). OS continua `Finalizada`, a entrega fica bloqueada e o Billing permite nova cobrança |

O último caso segue a classificação clássica de Sagas: até o fim da execução os passos são **compensáveis**; a execução é o **ponto sem volta** (*pivot*); a cobrança depois dela é **repetível** (*retriable*), ou seja, tenta-se de novo até dar certo, em vez de desfazer.

Toda compensação é **idempotente** e fica registrada no histórico da OS.

### Timeouts

| Espera | Timeout | Ao expirar |
|---|---|---|
| Resposta do cliente ao orçamento | Configurável (padrão de 7 dias; minutos na demonstração) | Tratado como recusa |
| Resposta de um serviço a um comando | Configurável (padrão de 5 minutos) | Reenvio do comando, depois compensação ([F4-29](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/160)) |
| Chegada de peças em falta | Sem timeout | Cancelamento manual permitido |

### Persistência e concorrência

- O estado de cada Saga é uma linha no banco do OS Service, identificada pelo **id da OS** (que também é o `correlationId` das mensagens).
- **Concorrência otimista** (coluna de versão): se dois eventos da mesma OS chegam juntos, um deles falha na gravação e é reprocessado, sem corromper o estado.
- O estado sobrevive a restart do pod e a reentregas de mensagem.

### Em aberto: momento do pagamento

Hoje a OS é paga **depois de finalizada** (`FinalizarServico` → `RegistrarPagamento` → `Entregar`).

| | **Opção A — pagamento no fim (proposta)** | Opção B — pagamento na aprovação |
|---|---|---|
| Regra de negócio | Inalterada | **Muda**: precisa ser formalizada antes de implementar |
| Compensação de pagamento | OS aguarda pagamento, sem estorno | Estorno no Mercado Pago se faltar estoque ou a execução falhar ([F4-21](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/152)) |
| Demonstração no vídeo | Compensações de recusa, estoque e execução | As mesmas, mais o estorno |
| Esforço | Menor | Maior (estorno, conciliação) |

A Opção A é a proposta porque não altera a regra de negócio e já oferece três cenários de compensação para demonstrar.

## Alternativas consideradas

### Saga coreografada

Cada serviço reage aos eventos dos outros, sem coordenador: a Execução publica o diagnóstico, o Billing reage gerando o orçamento, a Execução reage à aprovação reservando peças, e assim por diante.

Prós: nenhum ponto central; acoplamento menor ao OS Service; menos código no início.

Contras: o fluxo fica implícito, espalhado nos consumidores dos três serviços; a compensação exige que cada serviço saiba quais eventos a disparam; Billing e Execução passam a depender dos eventos um do outro; não existe um lugar onde se consulta "em que passo está a OS". Com três serviços e quatro compensações, a lógica distribuída fica mais difícil de testar em BDD e de mostrar no vídeo.

### Orquestrador como serviço separado

Um quarto serviço só para coordenar. Isola a orquestração, mas cria mais um repositório, pipeline e deploy, e duplicaria o papel do OS Service como dono do status. Não compensa dentro do prazo.

### Transação distribuída (two-phase commit)

Não atende o requisito (o enunciado pede Saga), exige que os bancos participem de um protocolo comum (inviável entre PostgreSQL e DynamoDB) e bloqueia recursos enquanto espera, o que não serve para um fluxo que inclui dias aguardando a aprovação do cliente.

## Consequências

### Positivas

- O fluxo completo da OS está descrito num lugar só, que é também o que se demonstra no vídeo
- Falhas têm compensação explícita e testável
- O estado de cada OS em cada passo é consultável e rastreável no Datadog pelo `correlationId`

### Negativas

- **O OS Service fica mais pesado.** Além do cadastro e do status, passa a conter a orquestração, que é a parte mais complexa da fase.
- **O OS Service é ponto de coordenação.** Se ele estiver fora do ar, as Sagas param de avançar (não se perdem: as mensagens esperam na fila).
- **Mais mensagens.** Cada passo é um comando e uma resposta, em vez de só um evento.
- **Consistência eventual.** Entre um passo e outro, os serviços podem mostrar estados diferentes da mesma OS por alguns instantes.

### Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Mensagem duplicada faz o passo rodar duas vezes | Consumidores idempotentes e transições validadas pelo estado atual ([ADR-0005](0005-mensageria-sns-sqs.md)) |
| Mensagem fora de ordem | O orquestrador ignora eventos que não fazem sentido no estado atual e registra o descarte |
| Saga parada esperando resposta que nunca vem | Timeouts com reenvio e compensação; alerta no dashboard ([F4-29](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/160)) |
| Compensação que também falha | Retentativa com backoff, DLQ e reprocessamento manual documentado |
| Mudança de regra de negócio sem formalização | O momento do pagamento fica em aberto até decisão formal (Opção A como padrão) |

## Pontos para decidir na reunião

1. Orquestração (proposta) ou coreografia?
2. Momento do pagamento: Opção A (proposta) ou B?
3. Timeout de aprovação do orçamento: 7 dias está bom?
4. Cancelamento manual durante a execução deve ser permitido?

## Referências

- [Épico E6 — Saga Pattern](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/127)
- [ADR-0003 — Divisão em microsserviços](0003-divisao-em-microsservicos.md)
- [ADR-0005 — Mensageria](0005-mensageria-sns-sqs.md)
- Microservices.io — [Pattern: Saga](https://microservices.io/patterns/data/saga.html)
- Chris Richardson, *Microservices Patterns*, cap. 4 — transações compensáveis, *pivot* e repetíveis
