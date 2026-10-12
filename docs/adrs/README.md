# Architecture Decision Records

Decisões arquiteturais **permanentes** do projeto — aquelas que, uma vez tomadas, são caras de reverter e precisam ser explicadas para quem chega depois.

## Índice

| ADR | Título | Status | Data |
|---|---|---|---|
| [0001](0001-segregacao-em-quatro-repositorios.md) | Segregar o monorepo em quatro repositórios | Proposta | 2026-08-11 |
| [0002](0002-estrategia-de-escalabilidade.md) | Escalar em duas camadas: HPA por utilização, Cluster Autoscaler por pod pendente | Aceita | 2026-09-04 |
| [0003](0003-divisao-em-microsservicos.md) | Dividir a aplicação em três microsserviços por etapa do atendimento | Proposta | 2026-10-12 |
| [0004](0004-saga-orquestrada.md) | Coordenar o fluxo da OS com uma Saga orquestrada no OS Service | Proposta | 2026-10-12 |
| [0005](0005-mensageria-sns-sqs.md) | Usar SNS e SQS para a comunicação assíncrona, com outbox e consumo idempotente | Proposta | 2026-10-12 |
| [0006](0006-banco-por-servico.md) | Banco próprio por serviço, com PostgreSQL no OS Service e na Execução e DynamoDB no Billing | Proposta | 2026-10-12 |

## ADR ou RFC?

| | ADR | RFC |
|---|---|---|
| Registra | Uma decisão já tomada | Uma proposta em discussão |
| Momento | Depois do consenso | Antes do consenso |
| Muda? | Vira "Substituída por", nunca se edita o conteúdo | Evolui durante o debate |
| Exemplo | Uso de HPA, padrão de comunicação | Escolha da nuvem, do banco, da estratégia de autenticação |

Os RFCs ficam em [`docs/rfcs/`](../rfcs/). Uma RFC aceita frequentemente gera uma ADR.

## Convenções

- Numeração sequencial de quatro dígitos, começando em `0001`
- Nome do arquivo: `NNNN-titulo-em-kebab-case.md`
- Use o [TEMPLATE.md](TEMPLATE.md)
- Status válidos: `Proposta`, `Aceita`, `Rejeitada`, `Substituída por ADR-NNNN`
- **ADR aceita não se edita.** Mudou de ideia? Escreva uma nova que substitui a anterior, e marque a antiga como substituída.
- Toda ADR entra por Pull Request, com revisão do grupo
