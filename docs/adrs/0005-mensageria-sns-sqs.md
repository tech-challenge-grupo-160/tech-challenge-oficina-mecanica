# ADR-0005: Usar SNS e SQS para a comunicação assíncrona, com outbox e consumo idempotente

| | |
|---|---|
| **Status** | Aceita em 2026-10-17 |
| **Data** | 2026-10-12 |
| **Decisores** | Grupo 160 |
| **Issue** | [#134](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/134) |

## Contexto

O enunciado da Fase 4 pede que a comunicação entre os microsserviços combine **APIs REST síncronas, quando necessário,** com **mensageria assíncrona** (RabbitMQ, Kafka, SQS etc.) para eventos e integração desacoplada. A mensageria também é o meio pelo qual a Saga ([ADR-0004](0004-saga-orquestrada.md)) envia comandos e recebe eventos.

Restrições do nosso ambiente:

- **AWS Academy Learner Lab**: não é possível criar roles IAM (os pods usam a `LabRole`) e o SCP já bloqueou ações inesperadas antes (ex.: `s3:GetBucketObjectLockConfiguration` na Fase 3).
- **Custo**: o ambiente sobe e desce em ciclos (`sobe-tudo.sh` / `derruba-tudo.sh`) para economizar crédito. Tudo que fica de pé cobra.
- **Prazo**: cinco semanas. Operar um broker é trabalho que não aparece na avaliação.

Mensageria sem cuidado gera dois problemas clássicos que a Saga não tolera:

1. **Evento perdido**: o serviço grava no banco e cai antes de publicar.
2. **Evento duplicado**: o broker entrega a mesma mensagem mais de uma vez (SQS garante entrega *pelo menos uma vez*).

## Decisão

Usar **Amazon SNS** (tópicos) e **Amazon SQS** (filas), com **outbox transacional** na publicação e **consumo idempotente**, usando **MassTransit** como biblioteca no .NET.

### Topologia

- **Eventos** (algo aconteceu): publicados num **tópico SNS do produtor**. Cada interessado tem **sua própria fila SQS** assinada no tópico. Quem publica não sabe quem consome.
- **Comandos** (faça algo): enviados direto para a **fila do serviço** que executa. Usados pelo orquestrador da Saga.
- **Toda fila tem DLQ** (*dead-letter queue*), para onde vão as mensagens que falharam depois de N tentativas.
- **Filas padrão** (não FIFO). A ordem é garantida pela máquina de estados da Saga, não pela fila.

```
              ┌── tópico os ────────► fila billing-os-eventos
OS Service ───┤                    └► fila execucao-os-eventos
   ▲          └── comandos ────────► fila billing / fila execucao
   │
   └──── fila os-eventos ◄── tópico billing ◄── Billing
                         ◄── tópico execucao ◄── Execução
```

### Envelope das mensagens

| Campo | Exemplo | Para quê |
|---|---|---|
| `messageId` | UUID | Idempotência |
| `type` | `billing.orcamento-aprovado` | Roteamento e versionamento |
| `version` | `1` | Evoluir o contrato sem quebrar consumidores |
| `correlationId` | id da OS | Liga todas as mensagens da mesma Saga |
| `causationId` | `messageId` que originou esta | Reconstruir a cadeia de causa |
| `occurredAt` | ISO 8601 UTC | Auditoria e ordenação |
| `producer` | `billing` | Diagnóstico |
| contexto de trace | atributos da mensagem | Trace distribuído no Datadog ([F4-31](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/162)) |

Nomes no formato `<serviço>.<fato-ou-ação>` em minúsculas com hífen. Mudança incompatível no payload gera nova `version`; o produtor publica as duas até os consumidores migrarem.

### Catálogo inicial de mensagens

| Mensagem | Tipo | Produtor | Consumidores |
|---|---|---|---|
| `os.aberta` | evento | OS Service | Billing, Execução (cópia de dados mínimos) |
| `os.cancelada` | evento | OS Service | Billing, Execução |
| `execucao.solicitar-diagnostico` | comando | OS Service | Execução |
| `execucao.diagnostico-concluido` | evento | Execução | OS Service |
| `billing.gerar-orcamento` | comando | OS Service | Billing |
| `billing.orcamento-enviado` | evento | Billing | OS Service |
| `billing.orcamento-aprovado` / `-recusado` | evento | Billing | OS Service |
| `execucao.reservar-pecas` | comando | OS Service | Execução |
| `execucao.pecas-reservadas` / `estoque-insuficiente` | evento | Execução | OS Service |
| `execucao.finalizada` / `abortada` | evento | Execução | OS Service |
| `billing.cobrar` | comando | OS Service | Billing |
| `billing.pagamento-confirmado` / `-recusado` | evento | Billing | OS Service |
| `billing.cancelar-orcamento` | comando (compensação) | OS Service | Billing |
| `execucao.liberar-reservas` | comando (compensação) | OS Service | Execução |

O catálogo detalhado (payload de cada mensagem) fica em `docs/mensageria/catalogo.md`, mantido junto com o código.

### Outbox transacional

O serviço grava a mudança de negócio **e a mensagem a publicar na mesma transação** do seu banco. Um processo em segundo plano lê as mensagens pendentes e publica no SNS/SQS. Se o pod cair entre a gravação e a publicação, a mensagem continua lá e é publicada depois.

| Serviço | Banco ([ADR-0006](0006-banco-por-servico.md)) | Implementação |
|---|---|---|
| OS Service, Execução | PostgreSQL | Outbox do MassTransit com EF Core |
| Billing | DynamoDB | Item de outbox gravado com `TransactWriteItems` junto do orçamento ou pagamento; publicador em segundo plano |

### Consumo idempotente

Cada consumidor registra o `messageId` das mensagens processadas (tabela ou item de *inbox*) **na mesma transação** do efeito. Mensagem repetida é reconhecida e descartada. Além disso, os handlers validam o estado atual (ex.: não aprovar um orçamento já aprovado).

### Falhas no consumo

- Retentativa com intervalo crescente (ex.: 1 s, 5 s, 15 s) dentro do serviço.
- Esgotadas as tentativas, a mensagem vai para a **DLQ** (`maxReceiveCount` do SQS).
- DLQ com mensagem gera alerta no Datadog; o reprocessamento é manual e documentado ([F4-29](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/160)).

### Quando usar REST síncrono

Só quando **quem chama precisa da resposta para continuar** e o dado é do outro serviço. Hoje o único caso é a validação do catálogo na abertura da OS (ADR-0003).

- Chamada interna pelo DNS do cluster, sem passar pelo API Gateway.
- Timeout curto, retentativa limitada e *circuit breaker* (`Microsoft.Extensions.Http.Resilience`).
- **Nunca** para mudar estado em outro serviço: mudança de estado é sempre por comando ou evento.

### Por que MassTransit

- Já traz transporte para SNS/SQS, **máquina de estados para Saga** com persistência em EF Core e **outbox transacional**, que são as três peças desta fase.
- Cria filas, tópicos e assinaturas a partir do código, ou se adapta à topologia criada pelo Terraform.
- **Atenção à licença:** confirmar antes de adotar que a versão usada é a open source (a linha v8). Se a licença for um problema, a alternativa é o AWS SDK direto, com outbox e máquina de estados próprios (mais código).

### Desenvolvimento local

LocalStack no `docker-compose` emulando SNS, SQS e DynamoDB, para rodar os três serviços sem AWS.

## Alternativas consideradas

### RabbitMQ no cluster

Prós: sem custo extra de serviço AWS, fácil de rodar localmente, muito bem suportado pelo MassTransit.

Contras: é mais um componente para operar no EKS (StatefulSet, volume persistente, credenciais) e para subir e derrubar nos scripts. Não aproveita nada que a AWS já gerencia. **Fica como plano B** caso o spike ([F4-05](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/136)) mostre que o Learner Lab bloqueia SNS/SQS.

### Kafka (Amazon MSK)

Pensado para alto volume e reprocessamento de histórico, o que não precisamos. O cluster tem custo fixo alto e o tempo de criação é longo, ruim para o ciclo de subir e derrubar.

### Amazon EventBridge

Ótimo roteamento de eventos, mas não substitui as filas (os consumidores continuariam precisando de SQS) e tem menos suporte nas bibliotecas .NET de Saga.

### Só REST síncrono

Não atende o enunciado e acoplaria a disponibilidade dos serviços: se o Billing cai, a abertura da OS falha.

## Consequências

### Positivas

- Sem broker para operar: SNS e SQS são gerenciados e cobram por uso (centavos por dia no nosso volume)
- Um serviço fora do ar não derruba os outros; as mensagens esperam na fila
- Outbox e idempotência eliminam as duas falhas clássicas (mensagem perdida e duplicada)
- Correlação pela OS em todas as mensagens, o que facilita o rastreamento e o vídeo

### Negativas

- **Mais infraestrutura por serviço**: tópico, filas e DLQs no Terraform de cada repositório
- **Debug mais difícil** que uma chamada direta: é preciso olhar filas, DLQs e traces
- **Dependência da AWS no desenvolvimento**, contornada com LocalStack
- **Outbox no DynamoDB é código nosso**, já que o MassTransit não cobre esse caso

### Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Pods sem credencial para acessar SNS/SQS (hop limit do IMDS, como no Cluster Autoscaler da Fase 3) | Spike [F4-05](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/136) antes de construir; launch template com hop limit 2 |
| SCP do Learner Lab bloquear alguma ação de SNS/SQS | Validado no spike; plano B é RabbitMQ no cluster |
| Contrato de mensagem quebrado entre serviços | Campo `version`, catálogo versionado e testes de contrato no CI |
| Filas acumulando mensagens sem ninguém perceber | Monitores no Datadog para idade da mensagem mais antiga e DLQ não vazia |

## Decisões da reunião (2026-10-17)

1. **SNS + SQS**, com RabbitMQ no cluster como plano B se o spike ([F4-05](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/136)) falhar.
2. **MassTransit**, condicionado a confirmar que a versão adotada é open source; se não for, AWS SDK direto.
3. Quem cria a topologia (Terraform ou biblioteca) fica para a [F4-11](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/142).

## Referências

- [Épico E2 — Infraestrutura e Mensageria](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/123)
- [ADR-0004 — Saga orquestrada](0004-saga-orquestrada.md)
- Microservices.io — [Transactional outbox](https://microservices.io/patterns/data/transactional-outbox.html) e [Idempotent consumer](https://microservices.io/patterns/communication-style/idempotent-consumer.html)
- [Amazon SQS — Dead-letter queues](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-dead-letter-queues.html)
- [MassTransit — Amazon SQS](https://masstransit.io/documentation/transports/amazon-sqs)
