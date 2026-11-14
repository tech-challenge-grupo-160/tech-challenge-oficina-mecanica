# C4 — contexto (Fase 4)

Quem usa o sistema e com quais sistemas externos ele conversa. Para o PDF, exporte a
imagem como descrito em [Exportar para imagem](../README.md#exportar-para-imagem).

```mermaid
C4Context
    title Contexto - Sistema de Gestao da Oficina Mecanica (Fase 4)

    Person(cliente, "Cliente", "Autentica pelo CPF, acompanha a OS, aprova o orcamento e paga")
    Person(funcionario, "Funcionario da oficina", "Recepcao, mecanico e gestor")

    System(oficina, "Sistema de Gestao da Oficina", "OS Service, Billing e Execucao na AWS, coordenados por uma Saga orquestrada")

    System_Ext(mp, "Mercado Pago", "Checkout Pro e webhook de pagamento")
    System_Ext(dd, "Datadog", "Logs, metricas e APM")
    System_Ext(gh, "GitHub Actions e SonarCloud", "CI/CD, quality gate e deploy")

    Rel(cliente, oficina, "Consulta OS, aprova orcamento", "HTTPS + JWT")
    Rel(funcionario, oficina, "Abre OS, diagnostica, executa e entrega", "HTTPS + JWT")
    Rel(oficina, mp, "Cria a cobranca", "HTTPS")
    Rel(mp, oficina, "Notifica o pagamento", "webhook")
    Rel(oficina, dd, "Envia logs e metricas", "agente")
    Rel(gh, oficina, "Publica imagens e aplica a infraestrutura", "Terraform, kubectl")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

| Elemento | Papel |
|---|---|
| Cliente | Autentica pelo CPF na Lambda, acompanha a OS e decide o orçamento |
| Funcionário | Opera a OS: abertura, diagnóstico, reparo e entrega |
| Mercado Pago | Recebe a cobrança do Billing e avisa o resultado por webhook |
| Datadog | Recebe logs estruturados (com `correlationId` e número da OS) e métricas |
| GitHub Actions e SonarCloud | Pipeline de cada serviço: testes, quality gate de 80%, imagem no ECR e deploy no EKS |
