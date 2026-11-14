# Implantação na AWS (Fase 4)

Onde cada peça roda em cada ambiente (dev, hom e prod), e quais repositórios as
provisionam. Substitui, para a aplicação, o
[diagrama da Fase 3](../fase-3/aws-deployment.md), que tinha uma API só. Para o PDF,
exporte a imagem como descrito em [Exportar para imagem](../README.md#exportar-para-imagem).

```mermaid
flowchart TB
    usuario(["Cliente e funcionarios"])
    mp["Mercado Pago"]
    dd["Datadog"]
    gh["GitHub Actions<br/>pipeline reutilizavel + Terraform"]

    subgraph aws["AWS us-east-1 · um conjunto por ambiente (dev, hom, prod)"]
        gw["API Gateway HTTP API<br/>POST /auth · POST /api/v1/auth/login<br/>POST /api/v1/pagamentos/webhook<br/>ANY /api/v1/{proxy+} com authorizer"]
        lambda["Lambda tc-grupo160-auth<br/>CPF para JWT + authorizer"]
        ecr["ECR<br/>imagens por SHA"]
        secrets["Secrets Manager<br/>banco, JWT, mensageria, Mercado Pago"]

        subgraph vpc["VPC · subnets privadas"]
            link["VPC Link"]
            alb["ALB interno<br/>regras por caminho"]

            subgraph eks["EKS · node group gerenciado + Cluster Autoscaler"]
                os["OS Service<br/>Deployment + HPA<br/>NodePort 30080"]
                billing["Billing<br/>Deployment + HPA<br/>NodePort 30081"]
                exec["Execucao<br/>Deployment + HPA<br/>NodePort 30082"]
            end

            osdb[("RDS PostgreSQL<br/>OS Service")]
            execdb[("RDS PostgreSQL<br/>Execucao")]
        end

        subgraph msg["Mensageria"]
            topicos{{"SNS<br/>os-events · billing-events · execucao-events"}}
            filas[["SQS<br/>15 filas, uma por tipo de mensagem<br/>+ 15 DLQs"]]
        end

        dynamo[("DynamoDB<br/>tabela do Billing")]
    end

    usuario --> gw
    gw -. autoriza .-> lambda
    gw --> link --> alb
    alb --> os
    alb --> billing
    alb --> exec

    os --> osdb
    exec --> execdb
    billing --> dynamo

    os & billing & exec <--> topicos
    topicos --> filas
    filas --> os & billing & exec

    billing <--> mp
    eks -. logs e metricas .-> dd
    eks -. le segredos .-> secrets
    gh -- imagem --> ecr
    gh -- kubectl apply --> eks
    ecr --> eks
```

| Repositório | O que provisiona |
|---|---|
| [tech-challenge-infra-k8s](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s) | VPC, EKS, ALB, API Gateway, VPC Link, authorizer e segredos compartilhados |
| [tech-challenge-infra-database](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-database) | RDS do OS Service e RDS da Execução |
| [tech-challenge-lambda-auth](https://github.com/tech-challenge-grupo-160/tech-challenge-lambda-auth) | Código das funções Lambda (autenticação por CPF e authorizer) |
| OS Service, Billing e Execução (`infra/`) | Tópico SNS de cada um, filas, DLQs e segredo de mensageria; a tabela DynamoDB fica no Billing |
| [tech-challenge-pipelines](https://github.com/tech-challenge-grupo-160/tech-challenge-pipelines) | Pipeline reutilizável: testes, SonarCloud, imagem no ECR e deploy no EKS |

O ciclo de subir e derrubar os ambientes no Learner Lab está em
[CICLO-DE-VIDA.md](../../CICLO-DE-VIDA.md).
