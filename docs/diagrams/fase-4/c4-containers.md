# C4 — containers (Fase 4)

Os três microsserviços, seus bancos e a mensageria entre eles. Para o PDF, exporte a
imagem como descrito em [Exportar para imagem](../README.md#exportar-para-imagem).

```mermaid
C4Container
    title Containers - Oficina Mecanica (Fase 4)

    Person(usuario, "Cliente e funcionarios")
    System_Ext(mp, "Mercado Pago", "Checkout Pro")
    System_Ext(dd, "Datadog", "Logs, metricas e APM")

    Boundary(aws, "AWS us-east-1") {
        Container(gw, "API Gateway HTTP API", "AWS", "POST /auth publica; /api/v1/* com authorizer JWT")
        Container(lambda, "Lambda de autenticacao", ".NET", "CPF para JWT e authorizer")
        Container(alb, "ALB interno", "AWS, via VPC Link", "Roteia por caminho para cada servico")

        Container(os, "OS Service", ".NET 10 no EKS", "Cliente, veiculo, OS, status e historico; orquestra a Saga")
        Container(billing, "Billing", ".NET 10 no EKS", "Orcamento e pagamento")
        Container(exec, "Execucao", ".NET 10 no EKS", "Catalogo, diagnostico, estoque e reparo")

        ContainerQueue(bus, "Mensageria", "SNS + SQS + DLQ", "3 topicos; 1 fila por tipo de mensagem")

        ContainerDb(osdb, "Banco do OS", "PostgreSQL (RDS)")
        ContainerDb(billdb, "Banco do Billing", "DynamoDB")
        ContainerDb(execdb, "Banco da Execucao", "PostgreSQL (RDS proprio)")
    }

    Rel(usuario, gw, "HTTPS")
    Rel(gw, lambda, "autentica e autoriza")
    Rel(gw, alb, "/api/v1/*")
    Rel(alb, os, "demais rotas")
    Rel(alb, billing, "/orcamentos, /pagamentos")
    Rel(alb, exec, "/catalogo, /diagnosticos, /estoque, /execucoes")

    Rel(os, bus, "comandos e eventos", "outbox")
    Rel(billing, bus, "eventos e comandos", "outbox")
    Rel(exec, bus, "eventos e comandos", "outbox")
    Rel(os, exec, "consulta o catalogo na abertura", "HTTP")

    Rel(os, osdb, "EF Core")
    Rel(billing, billdb, "TransactWriteItems")
    Rel(exec, execdb, "EF Core")

    Rel(billing, mp, "cobranca e webhook", "HTTPS")
    Rel(os, dd, "logs e metricas")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

Regras que o desenho assume ([ADR-0003](../../adrs/0003-divisao-em-microsservicos.md) a
[ADR-0006](../../adrs/0006-banco-por-servico.md)):

- **Nenhum serviço acessa o banco de outro.** Dado de outro serviço chega por
  mensagem ou, na abertura da OS, pela consulta ao catálogo da Execução.
- **Só o OS Service altera o status da OS**, sempre em reação a eventos.
- **Mensageria com outbox e consumo idempotente.** Comandos vão por envio
  direto à fila do serviço de destino; eventos vão pelo tópico do produtor, e a
  fila do OS filtra pelo atributo `tipo`. Toda fila tem DLQ.
- Os três serviços mandam logs e métricas ao Datadog. A seta sai só do OS para
  o desenho não ficar poluído.
