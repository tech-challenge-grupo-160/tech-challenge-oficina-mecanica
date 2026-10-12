# ADR-0006: Banco próprio por serviço, com PostgreSQL no OS Service e na Execução e DynamoDB no Billing

| | |
|---|---|
| **Status** | Proposta |
| **Data** | 2026-10-12 |
| **Decisores** | Grupo 160 |
| **Issue** | [#135](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/135) |

## Contexto

O enunciado da Fase 4 exige que **cada microsserviço tenha banco de dados próprio**, com **pelo menos um banco relacional (SQL) e pelo menos um não relacional (NoSQL)**, e que **nenhum serviço acesse diretamente o banco de outro**.

Hoje há um único PostgreSQL 16 no RDS (Multi-AZ), provisionado pelo repositório `tech-challenge-infra-database`, com todas as tabelas da aplicação, acessado via EF Core.

Pela [ADR-0003](0003-divisao-em-microsservicos.md), os dados ficam assim:

| Serviço | Dados | Perfil de acesso |
|---|---|---|
| OS Service | Cliente, veículo, OS, histórico, notificações, usuários, **estado da Saga** | Relacional, com chaves estrangeiras e consultas de monitoramento |
| Execução | Catálogo de serviços e peças, estoque, movimentações, pedidos de compra, itens do diagnóstico | Relacional, **com reserva de estoque concorrente** entre OS |
| Billing | Orçamento (com cópia congelada dos itens), pagamentos, notificações do Mercado Pago | Busca por chave: "orçamento da OS X", "pagamento Y do Mercado Pago" |

O planejamento inicial propunha o DynamoDB na Execução. Com a divisão da ADR-0003, a Execução herdou os dados mais relacionais do sistema, o que mudou a recomendação.

## Decisão

| Serviço | Banco | Onde é provisionado |
|---|---|---|
| **OS Service** | PostgreSQL — o RDS atual | `tech-challenge-infra-database` (sem mudança) |
| **Execução** | PostgreSQL — **instância RDS própria** | Pasta `infra/` do repositório da Execução, reaproveitando o módulo do `infra-database` |
| **Billing** | **Amazon DynamoDB** | Pasta `infra/` do repositório do Billing |

Cada serviço tem **credenciais próprias no Secrets Manager**, e nenhum serviço recebe credencial do banco de outro.

### Por que PostgreSQL na Execução

- **É o serviço que mais herda código pronto.** Peças, serviços, pedidos de compra, movimentações e itens já têm entidades, configurações e migrations em EF Core e PostgreSQL. Mantê-lo relacional reaproveita esse código e os testes.
- **Reserva de estoque é um problema clássico de transação relacional.** Reservar várias peças de uma OS sem que outra OS pegue as mesmas é simples com transação e concorrência otimista no PostgreSQL.
- **O outbox do MassTransit funciona com EF Core** ([ADR-0005](0005-mensageria-sns-sqs.md)), sem código próprio.

### Por que DynamoDB no Billing

- **O Billing é código novo.** Escrever acesso ao DynamoDB nele não reescreve nada que já existe.
- **Os acessos são por chave**: orçamento por OS, pagamento por id do Mercado Pago, deduplicação de notificações do webhook. É o caso de uso ideal do DynamoDB.
- **O orçamento é um documento**: cabeçalho e a lista congelada de itens, lidos e gravados juntos.
- **Custo zero parado** no modo sob demanda, o que combina com o ciclo de subir e derrubar o ambiente.
- **TTL nativo** para expirar registros de deduplicação de webhook e de mensagens processadas.

### Modelo inicial do Billing (tabela única)

| Item | PK | SK | Atributos principais |
|---|---|---|---|
| Orçamento | `OS#<osId>` | `ORCAMENTO#<versao>` | status, itens, total, enviadoEm, expiraEm, decididoEm |
| Pagamento | `OS#<osId>` | `PAGAMENTO#<mpPaymentId>` | status, valor, método, criadoEm, confirmadoEm |
| Deduplicação de webhook | `WEBHOOK#<notificationId>` | `-` | recebidoEm, TTL |
| Mensagem processada (inbox) | `MSG#<messageId>` | `-` | processadaEm, TTL |
| Outbox | `OUTBOX#<particao>` | `<timestamp>#<messageId>` | tipo, payload, publicadaEm |

**Índice secundário** `GSI1`: PK `MP#<mpPaymentId>`, para o webhook localizar o pagamento só pelo id do Mercado Pago.

| Padrão de acesso | Como |
|---|---|
| Orçamento atual de uma OS | `Query` em `OS#<osId>` com `begins_with(SK, "ORCAMENTO#")`, último |
| Pagamentos de uma OS | `Query` em `OS#<osId>` com `begins_with(SK, "PAGAMENTO#")` |
| Pagamento a partir do webhook | `Query` no `GSI1` |
| Aprovar orçamento sem corrida | `UpdateItem` com condição `status = Enviado` |
| Gravar orçamento e mensagem juntos | `TransactWriteItems` (orçamento + outbox) |

### Ambientes e custo

- **RDS da Execução**: instância pequena (classe `t4g.micro`), **Single-AZ em dev e homolog** e **Multi-AZ só em produção**, na mesma linha do RDS atual. Estimativa a confirmar na calculadora da AWS: algo como US$ 0,40/dia por ambiente em Single-AZ, o dobro em Multi-AZ.
- **DynamoDB**: sob demanda, com custo desprezível no nosso volume e zero com o ambiente parado. Point-in-time recovery ligado só em produção.
- O `sobe-tudo.sh` e o `derruba-tudo.sh` passam a incluir os dois bancos novos ([F4-39](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/170)).

## Alternativas consideradas

### DynamoDB na Execução, PostgreSQL no Billing (proposta original do planejamento)

Atenderia ao requisito igualmente. Não foi escolhida porque obrigaria a **reescrever para DynamoDB o código que mais existe hoje** (catálogo, estoque, pedidos de compra) e a resolver a reserva de várias peças com escritas condicionais, enquanto o Billing, que é código novo e acessado por chave, ficaria no relacional sem precisar dele.

### Database separado na instância RDS atual, em vez de instância própria para a Execução

Mais barato: um `CREATE DATABASE` com usuário próprio na instância do OS Service, sem custo adicional. Isola os dados logicamente e atende a regra de nenhum serviço acessar o banco do outro.

Não é a proposta porque o enunciado pede que cada serviço tenha **sua própria infraestrutura e banco de dados**, e uma instância compartilhada pode ser lida como descumprimento. Se o custo apertar, é a primeira alternativa a adotar, registrando a mudança aqui.

### MongoDB (Atlas ou no cluster) no Billing

Teria suporte de outbox no MassTransit, eliminando código próprio. Não foi escolhido porque traz uma conta externa (Atlas) ou mais um componente com estado para operar no EKS, enquanto o DynamoDB é gerenciado, já está em uso no projeto (lock do Terraform) e não cobra parado.

## Consequências

### Positivas

- Requisito de SQL + NoSQL atendido com cada banco no serviço que mais se beneficia dele
- Reaproveitamento máximo do código EF Core na extração da Execução
- Billing sem custo com o ambiente parado
- Isolamento real: credenciais separadas e nenhum acesso cruzado

### Negativas

- **Mais um RDS** para provisionar, subir, derrubar e pagar
- **Outbox no DynamoDB é código próprio** no Billing (ADR-0005)
- **Dois modelos de persistência** no time: quem trabalhar no Billing precisa aprender a modelar para o DynamoDB
- **Relatórios que cruzam serviços** (ex.: faturamento por tipo de serviço) não saem mais de uma consulta só; dependem de eventos ou composição

### Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Modelo do DynamoDB não atender um acesso descoberto depois | Padrões de acesso listados acima revisados antes de implementar; GSI pode ser adicionado sem migração |
| Custo do segundo RDS | Single-AZ fora de produção; alternativa de database compartilhado documentada |
| Pod sem credencial para o DynamoDB | Spike [F4-05](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/136) |
| Migração dos dados atuais | Script idempotente por serviço ([F4-18](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/149)) |

## Impacto no board

Se esta ADR for aceita, duas issues trocam de serviço:

- [F4-12](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/143) "Terraform do banco do Billing (PostgreSQL)" passa a ser o RDS da Execução
- [F4-13](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/144) "Terraform das tabelas DynamoDB da Execução" passa a ser a tabela do Billing

## Pontos para decidir na reunião

1. DynamoDB no Billing e PostgreSQL na Execução (proposta), ou a divisão original?
2. Instância RDS própria para a Execução (proposta) ou database separado na instância atual?

## Referências

- [Épico E2 — Infraestrutura e Mensageria](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/123)
- [ADR-0003 — Divisão em microsserviços](0003-divisao-em-microsservicos.md)
- [ADR-0005 — Mensageria](0005-mensageria-sns-sqs.md)
- Microservices.io — [Database per service](https://microservices.io/patterns/data/database-per-service.html)
- [Amazon DynamoDB — Best practices for designing and architecting](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/best-practices.html)
