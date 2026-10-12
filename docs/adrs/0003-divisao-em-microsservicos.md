# ADR-0003: Dividir a aplicação em três microsserviços por etapa do atendimento

| | |
|---|---|
| **Status** | Proposta |
| **Data** | 2026-10-12 |
| **Decisores** | Grupo 160 |
| **Issue** | [#132](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/132) |

## Contexto

Até a Fase 3 a regra de negócio vive numa única API .NET, com um único banco PostgreSQL. A Fase 4 exige refatorar para **no mínimo três microsserviços**, cada um com **repositório, infraestrutura e banco de dados próprios**, sendo que **nenhum serviço pode acessar o banco de outro**. O enunciado sugere três responsabilidades: Ordem de Serviço, Orçamento e Pagamento (com Mercado Pago) e Execução e Produção.

Antes de criar repositórios ou escrever código, é preciso decidir **qual serviço é dono de cada dado de hoje**. Essa decisão é a base das outras três ADRs da fase (Saga, mensageria e banco por serviço) e da extração do monólito.

### Como o fluxo funciona hoje

Tudo gira em torno da entidade `OrdemDeServico`, que acumula papéis que agora vão para serviços diferentes:

1. **Abertura** — a OS nasce `Recebida`, ligada a `Cliente` e `Veiculo`, com código de acompanhamento público.
2. **Diagnóstico** — `EmDiagnostico`; o mecânico adiciona serviços e peças. Cada item guarda o **preço congelado** no momento da inclusão (`OrdemDeServicoServico.Preco`, `OrdemDeServicoPeca.Preco`).
3. **Orçamento** — não existe como entidade: é a própria OS com `ValorTotal` e `OrcamentoEnviadoEm`. Ao finalizar o diagnóstico ela vai para `AguardandoAprovacao` e o cliente é notificado (`NotificacaoCliente`).
4. **Aprovação** — `ResponderOrdemDeServicoCommandHandler` e `AprovarOrdemDeServicoCommandHandler` verificam o estoque **na mesma transação**: se falta peça, a OS vai para `AguardandoEstoque` e gera `PedidoCompra`; se não falta, dá baixa no estoque (`MovimentacaoEstoque`) e vai para `EmExecucao`. Recusa cancela a OS.
5. **Execução** — `Finalizada` ao concluir o reparo.
6. **Pagamento** — `RegistrarPagamento` grava `DataPagamento` na própria OS.
7. **Entrega** — `Entregue`, só com pagamento registrado.

O passo 4 é o melhor exemplo do que muda: hoje aprovação, estoque e status são atualizados numa transação só. Separados em serviços, isso passa a ser coordenado pela Saga ([ADR-0004](0004-saga-orquestrada.md)).

## Decisão

Dividir em **três serviços, um por etapa do atendimento**: o **OS Service** cuida do cliente e da OS como um todo, o **Billing** cuida do dinheiro e a **Execução** cuida do trabalho na oficina (diagnóstico, reparo, catálogo, peças e estoque).

### Os serviços

| Serviço | Repositório | Responsabilidade |
|---|---|---|
| **OS Service** | `tech-challenge-oficina-mecanica` (atual, mantido) | Clientes, veículos, abertura da OS, status e histórico, acompanhamento público, notificações ao cliente, login de funcionários e **orquestração da Saga** |
| **Billing** | `tech-challenge-billing` _(nome a confirmar)_ | Orçamento (geração, envio, aprovação e recusa), pagamento via Mercado Pago, estorno |
| **Execução** | `tech-challenge-execucao` _(nome a confirmar)_ | Diagnóstico e reparo, fila de execução, catálogo de serviços e peças, estoque, reserva de peças e pedidos de compra |

O repositório atual **não é renomeado**: ele vira o OS Service. Renomear quebraria automações, URLs e referências da Fase 3 sem ganho para a avaliação.

### Dono de cada dado

| Dado de hoje | Dono | Observação |
|---|---|---|
| `Cliente`, `Veiculo` | OS Service | Outros serviços recebem só o mínimo necessário, por evento |
| `OrdemDeServico` (cabeçalho, status, datas, código de acompanhamento) | OS Service | Único serviço que **escreve** o status da OS, sempre em reação a eventos |
| `OrdemServicoHistorico` | OS Service | Alimentado pelos eventos de todos os serviços |
| `NotificacaoCliente` | OS Service | Quem tem o contato do cliente envia a notificação; Billing e Execução só publicam o fato |
| `Usuario` (login de funcionários) | OS Service | O JWT emitido continua válido nos três serviços (mesma chave de assinatura e authorizer no API Gateway) |
| `Servico` (catálogo) | Execução | Quem diagnostica escolhe do catálogo |
| `Peca` (catálogo e estoque) | Execução | |
| `MovimentacaoEstoque`, `PedidoCompra` | Execução | Referenciam a OS só pelo id |
| `OrdemDeServicoServico`, `OrdemDeServicoPeca` (itens do diagnóstico) | Execução | O Billing guarda uma **cópia congelada** dos itens no orçamento |
| Orçamento (hoje `ValorTotal` e `OrcamentoEnviadoEm` na OS) | Billing | Vira entidade própria |
| Pagamento (hoje `DataPagamento` na OS) | Billing | Vira entidade própria, com o id do pagamento no Mercado Pago |

O OS Service mantém `ValorTotal` e a data de pagamento como **cópia de leitura**, atualizada por evento, para não depender do Billing em cada consulta da OS e do acompanhamento público. A fonte da verdade é o Billing.

### Como cada serviço obtém dado que não é dele

Regra geral: **por evento, guardando uma cópia local mínima**. Chamada síncrona só quando a resposta é necessária na hora e o dado muda pouco.

| Quem precisa | De quê | Como |
|---|---|---|
| Execução | Placa, modelo e descrição da solicitação, para o mecânico | Cópia recebida no evento de OS aberta |
| Billing | Itens do diagnóstico com preço | Evento de diagnóstico concluído, com o preço congelado |
| Billing | Nome e e-mail do cliente, para a cobrança no Mercado Pago | Cópia recebida no evento de OS aberta |
| OS Service | Mudanças de status (orçamento, pagamento, execução) | Eventos do Billing e da Execução |
| OS Service | Validar serviços informados **na abertura** | **REST síncrono** ao catálogo da Execução |

Dados pessoais copiados para outros serviços se limitam ao necessário para a função de cada um. O CPF não sai do OS Service.

### O que acontece com os endpoints atuais

| Endpoint de hoje | Vai para |
|---|---|
| `/auth`, `/clientes`, `/veiculos`, `/acompanhamento-os` | OS Service |
| `/ordens-servico`: criar, consultar, listar, histórico, notificações, monitoramento, cancelar, entregar | OS Service |
| `/ordens-servico`: iniciar e finalizar diagnóstico, adicionar e remover serviços e peças, liberar execução, finalizar, estimativa de tempo, movimentações de estoque | Execução |
| `/ordens-servico`: aprovar, responder (cliente), registrar pagamento | Billing |
| `/servicos`, `/pecas`, `/pedidos-compra` | Execução |
| `/ordens-servico/{numero}/avancar-status` | **A revisar** — com a Saga, o status deixa de ser avançado manualmente |

### Esboço do fluxo entre os serviços

O detalhamento dos passos, compensações e timeouts é da [ADR-0004](0004-saga-orquestrada.md). Aqui fica só quem faz o quê:

```
OS Service        Execução                 Billing
abre a OS  ─────► diagnostica
                  (monta os itens) ──────► gera o orçamento
                                           cliente aprova ou recusa
           ◄──────────────────────────────  (recusa: OS cancelada)
                  reserva as peças
                  (falta: pedido de compra, OS aguardando estoque)
                  executa e finaliza ────► cobra (Mercado Pago)
entrega    ◄──────────────────────────────  pagamento confirmado
```

## Alternativas consideradas

### Diagnóstico no OS Service e Execução só depois da aprovação

Manteria os itens dentro da OS e reduziria a extração. Não foi escolhida porque o enunciado coloca o diagnóstico explicitamente na Execução ("atualizar status durante diagnóstico e reparos"), a Execução ficaria com pouca responsabilidade, e o OS Service continuaria sendo um pequeno monólito com catálogo, itens e status juntos.

### Catálogo e preços no Billing

Preço é assunto comercial, então o Billing seria um dono natural. Não foi escolhida porque o diagnóstico (na Execução) consulta o catálogo a cada item adicionado: isso viraria uma chamada síncrona entre serviços no caminho mais usado. Com o catálogo na Execução e o preço congelado no orçamento, o Billing não precisa consultar ninguém.

### Quatro serviços, com Catálogo e Estoque separados da Execução

Mais coeso, mas cada serviço novo custa repositório, pipeline, banco, 80% de cobertura e infraestrutura no Learner Lab. Com cinco semanas até a entrega, o mínimo de três serviços bem feitos vale mais que quatro incompletos. Pode ser revisto numa fase futura.

### Cliente e veículo em serviço próprio de cadastro

Mesmo argumento da alternativa anterior. Além disso, cliente e veículo são lidos principalmente na abertura e no acompanhamento da OS, que já estão no OS Service.

## Consequências

### Positivas

- Cada serviço tem uma pergunta de negócio clara: "em que pé está a OS?" (OS), "quanto custa e foi pago?" (Billing), "o que fazer no carro e com quais peças?" (Execução)
- O Billing gera o orçamento só com os dados do evento, sem consultar outros serviços
- O status da OS tem um único escritor, o que simplifica a Saga e evita conflitos
- Os três serviços seguem as responsabilidades sugeridas no enunciado, o que facilita a avaliação

### Negativas

- **Consistência eventual.** O status no OS Service fica alguns instantes atrás do que aconteceu no Billing ou na Execução. O acompanhamento público pode mostrar um status desatualizado por esse intervalo.
- **A aprovação deixa de ser atômica.** Hoje a aprovação, a baixa de estoque e a mudança de status ocorrem numa única transação; passam a ser passos da Saga, com compensação explícita.
- **Dados duplicados.** Cópias de cliente e veículo na Execução e no Billing, e de valor e pagamento no OS Service, que precisam ser mantidas por evento.
- **Consultas que antes eram um `JOIN` passam a ser compostas.** Ver a OS com os itens exige combinar dados de dois serviços.
- **A extração é grande.** O `ResponderOrdemDeServicoCommandHandler` e o `AprovarOrdemDeServicoCommandHandler` precisam ser desmontados, e os testes existentes reorganizados entre três repositórios.
- **Migração dos dados atuais** (seeds e massa de demonstração) para os novos bancos (F4-18).

### Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Evento perdido deixa o status da OS divergente | Outbox transacional e consumidor idempotente (F4-15); DLQ monitorada |
| Cópias de dados pessoais em vários bancos | Copiar só o necessário; CPF fica só no OS Service; logs com CPF mascarado |
| Reserva de estoque concorrente para duas OS | Reserva em transação com concorrência otimista na Execução (F4-23, [ADR-0006](0006-banco-por-servico.md)) |
| Fronteira errada descoberta durante a extração | Esta ADR fica como **Proposta** até o fim da Sprint 3; ajustes entram aqui antes de virar **Aceita** |
| Chamada síncrona ao catálogo indisponível na abertura | Timeout curto e abertura sem serviços pré-selecionados como alternativa |

## Pontos para decidir na reunião

1. Diagnóstico na Execução (proposta) ou no OS Service?
2. Catálogo de serviços e peças com preço na Execução (proposta) ou no Billing?
3. Notificações ao cliente centralizadas no OS Service (proposta)?
4. Nomes dos repositórios novos.
5. O endpoint `avancar-status` é removido ou mantido só para uso administrativo?

## Referências

- [Épico E1 — Arquitetura e Decisões](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/122)
- [ADR-0001 — Segregar o monorepo em quatro repositórios](0001-segregacao-em-quatro-repositorios.md)
- Enunciado do Tech Challenge — Fase 4 (SOAT)
- Microservices.io — [Database per service](https://microservices.io/patterns/data/database-per-service.html) e [Saga](https://microservices.io/patterns/data/saga.html)
