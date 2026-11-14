# Rodar os três serviços localmente

Como subir OS Service, Billing e Execução na sua máquina, com mensageria
(SNS/SQS) e DynamoDB pelo LocalStack, e exercitar o fluxo completo da OS,
inclusive as compensações da Saga. Não precisa de conta da AWS, de Mercado Pago
nem de Datadog.

> **Validado em 14/11/2026** no Windows 11 com Docker Desktop. Passaram de ponta
> a ponta, pelas APIs: o caminho feliz até a entrega, o pagamento recusado com
> nova cobrança, o orçamento recusado e a falta de estoque com reposição. No
> Linux, os composes já trazem o ajuste do `host.docker.internal`, mas ele não
> foi testado numa máquina Linux.

## Como as peças se ligam

| Repositório | O que sobe | Porta no seu computador |
|---|---|---|
| [tech-challenge-oficina-mecanica](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica) | LocalStack (SNS, SQS e DynamoDB, com tópico, 15 filas e assinaturas criados na subida), PostgreSQL do OS e a API do OS | API `8080`, LocalStack `4566`, banco `5432` |
| [tech-challenge-oficina-mecanica-execucao](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-execucao) | PostgreSQL da Execução (com o catálogo do seed) e a API | API `8082`, banco `5433` |
| [tech-challenge-oficina-mecanica-billing](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica-billing) | A API (a tabela DynamoDB é criada no LocalStack na subida) | API `8081` |

Os três composes conversam pelo `host.docker.internal`. O LocalStack fica no
compose do OS, então **o OS sobe primeiro**. Localmente, todos rodam em
`Development`: o Billing não chama o Mercado Pago, e a confirmação ou a recusa do
pagamento são simuladas por endpoint.

## Pré-requisitos

- **Docker** com Compose v2: Docker Desktop no Windows e no Mac, ou Docker Engine
  no Linux. Os containers usam cerca de 1,1 GB de memória.
- **Git** e as portas `8080`, `8081`, `8082`, `4566`, `5432` e `5433` livres.
- **Postman** (ou outro cliente HTTP) para o fluxo.

Os três repositórios lado a lado, na branch `develop`:

```bash
mkdir -p ~/source/repos/tech-challenge && cd ~/source/repos/tech-challenge
for r in oficina-mecanica oficina-mecanica-execucao oficina-mecanica-billing; do
  git clone https://github.com/tech-challenge-grupo-160/tech-challenge-$r.git
  git -C tech-challenge-$r switch develop
done
```

## Subindo

Em cada repositório, o `.env.example` já tem valores que funcionam localmente:
copie para `.env` (o Git ignora esse arquivo) e suba. A primeira subida compila
as imagens e leva alguns minutos.

**1. OS Service, com o LocalStack**

```bash
cd tech-challenge-oficina-mecanica
cp .env.example .env
docker compose up -d --build
```

Espere o LocalStack terminar de criar as filas:

```bash
docker logs oficina-mecanica-localstack-dev 2>&1 | grep "configurados"
```

A saída esperada é `SNS/SQS locais configurados na regiao us-east-1 para a conta 000000000000.`

**2. Execução**

```bash
cd ../tech-challenge-oficina-mecanica-execucao
cp .env.example .env
docker compose up -d --build
```

**3. Billing**

```bash
cd ../tech-challenge-oficina-mecanica-billing
cp .env.example .env
docker compose up -d --build
```

**Conferir**

```bash
for p in 8080 8081 8082; do echo "$p: $(curl -s -o /dev/null -w '%{http_code}' localhost:$p/health/live)"; done
```

Os três devem responder `200`. O Swagger de cada um fica em
`http://localhost:<porta>/swagger`.

## Rodando o fluxo

1. Importe no Postman a collection e o environment **local** de
   [docs/postman](postman/README.md).
2. No environment, preencha `senha` com `admin123` (usuário `admin` do seed de
   desenvolvimento).
3. Rode as pastas em ordem, de `00` a `06`. Pule o **Token do cliente pelo CPF**
   e o **Acompanhamento pelo cliente**: os dois dependem da Lambda e só funcionam
   na AWS.
4. Os passos entre serviços são assíncronos. Quando uma consulta ainda não
   mostra a mudança, espere um ou dois segundos e repita.

A pasta `01` cria um cliente e um veículo novos a cada execução (CPF e placa
gerados no pre-request), porque só pode haver uma OS ativa por cliente e
veículo.

Para cada cenário, na pasta `05 - Pagamento`:

| Cenário | O que fazer |
|---|---|
| Caminho feliz | **Simular confirmação** e depois **Entregar veículo** (pasta 06) |
| Pagamento recusado | **Simular recusa**. A entrega responde `400` e a OS segue `Finalizada`. Depois, **Pedir nova cobrança**, consulte o pagamento de novo (agora com `tentativa: 2`), simule a confirmação e entregue |
| Orçamento recusado | Na pasta `03`, **Recusar orçamento** em vez de aprovar. A OS fica `Cancelada` |
| Falta de estoque | No **Registrar resultado do diagnóstico**, peça mais de 100 unidades da peça `1000` (o estoque do seed). A OS fica `AguardandoEstoque` até o **Repor estoque** da pasta `04` |

O histórico da OS (`GET /ordens-servico/{id}/historico`) mostra cada passo da
Saga.

## Desligando

```bash
docker compose down        # em cada um dos três repositórios
docker compose down -v     # o mesmo, apagando também os bancos
```

## Quando dá errado

| Sintoma | Causa e solução |
|---|---|
| Login responde `400` com `IDX10720 ... key has '224' bits` | `JWT_SECRET` com menos de 32 caracteres no `.env` do OS. Use o valor do `.env.example` ou outro com 32 ou mais |
| Compose falha com `Defina ... no arquivo .env` | Falta o `.env`: `cp .env.example .env` no repositório |
| Billing ou Execução não recebem mensagens; a OS para no primeiro passo assíncrono | O LocalStack não está de pé. Suba o OS primeiro e confira o log acima. No Linux, confirme que o compose tem o `extra_hosts` com `host-gateway` (vem na `develop`) |
| Abrir OS responde `400` "Já existe uma ordem de serviço ativa" | Rode antes o **Criar cliente** e o **Criar veículo** da pasta `01`: o seed já tem OS ativas para os veículos dele |
| Porta ocupada | Troque `API_PORT` ou `DB_PORT` no `.env` daquele serviço. Se mudar a porta da Execução, ajuste `EXECUCAO_BASE_URL` no `.env` do OS |
| Um passo não avança | Veja o log do serviço que deveria agir: `docker logs oficina-mecanica-api-dev`, `oficina-mecanica-execucao-dev` ou `oficina-mecanica-billing-dev` |
| Banco inconsistente depois de trocar de branch | `docker compose down -v` e suba de novo |

O LocalStack está fixo na versão `4.14.0`, a última da linha Community antes da
exigência de token nas imagens de 2026. Não troque para `latest`.

Para subir os ambientes na AWS, o caminho é outro: [CICLO-DE-VIDA.md](CICLO-DE-VIDA.md).
