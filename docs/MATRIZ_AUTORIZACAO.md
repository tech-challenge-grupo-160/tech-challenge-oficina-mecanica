# Matriz de autorização

Classificação de todas as rotas da API entre públicas e protegidas, com o critério que sustenta cada decisão.

| | |
|---|---|
| **Issue** | [#44](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/44) (F3-10) |
| **Base** | Código da branch `develop` em 2026-08-26 |
| **Decisão de mecanismo** | [RFC-0002](rfcs/0002-autenticacao-por-cpf-e-api-gateway.md) |

Este documento define **o que** proteger. O **como** — API Gateway HTTP API com Lambda authorizer — está na RFC-0002. É insumo direto das issues [#42](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/42) (rotas do gateway) e [#43](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/43) (authorizer).

## Critério de sensibilidade

Cada rota responde a quatro perguntas, na ordem. A primeira que der "sim" define o nível.

| # | Pergunta | Nível resultante |
|---|---|---|
| 1 | Expõe detalhe de infraestrutura (banco, versão, exceção)? | **Interna** — não roteada pelo gateway |
| 2 | É pré-requisito para obter o token? | **Pública** — precisa ser alcançável sem token |
| 3 | Expõe dado pessoal do cliente, ou permite que ele aja sobre a própria OS? | **Cliente** — exige `role = Cliente` |
| 4 | Lê ou altera dado de negócio? | **Autenticada** — exige JWT válido |

### Níveis

| Nível | Exigência | No gateway |
|---|---|---|
| **Pública** | Nenhuma | Roteada, sem authorizer |
| **Autenticada** | JWT válido | Roteada, com authorizer |
| **Cliente** | JWT válido com `role = Cliente` | Roteada, com authorizer |
| **Interna** | Alcançável só dentro da VPC | **Não roteada** |

O nível **Cliente** é mais restritivo que **Autenticada**, não mais permissivo: exige a claim de papel, não só um token válido.

## Resumo

**53 rotas de controller**, em 8 controllers, mais 3 endpoints de health.

| Nível | Rotas | Onde |
|---|---|---|
| Pública | 1 | `POST /auth/login` |
| Cliente | 2 | Acompanhamento de OS e resposta ao orçamento |
| Autenticada | 50 | Os 6 controllers de gestão |
| Interna | 3 | `/health`, `/health/live`, `/health/ready` |

## Rotas públicas

| Método | Rota | Nível | Por quê |
|---|---|---|---|
| `POST` | `/api/v1/auth/login` | **Pública** | Critério 2 — é a rota que emite o token. Marcada com `[AllowAnonymous]` e limitada a 10 req/min por IP |

Com o API Gateway em produção, esta rota deixa de ser a porta de entrada principal: quem autentica por CPF chama a **Lambda**, não a API. O `/auth/login` permanece para o usuário administrativo do seed e para desenvolvimento local.

> ⚠️ **O caminho é `auth` em minúsculo, e isso importa no gateway.**
>
> O roteamento do API Gateway é **sensível a maiúsculas**; o do ASP.NET não é.
> Local, `/api/v1/Auth/login` e `/api/v1/auth/login` são a mesma coisa. Pelo
> gateway, só o segundo casa com a rota pública — o primeiro cai no
> `ANY /api/v1/{proxy+}`, recebe o authorizer e devolve **401**, o que parece
> credencial errada e não é.
>
> A caixa canônica vem do controller. O `AuthController` declara
> `[Route("api/v1/auth")]` explicitamente, em minúsculo. Os demais usam
> `[Route("api/v1/[controller]")]`, e aí o token preserva o nome da classe —
> `Clientes`, `Pecas`, `Servicos` **capitalizados estão corretos**.
>
> Para essas, a caixa não muda nada: todas caem no `{proxy+}` de qualquer jeito.
> A diferença só aparece na única rota com entrada pública própria no gateway.

## Rotas de nível Cliente

| Método | Rota | Controller | Por quê |
|---|---|---|---|
| `GET` | `/api/v1/acompanhamento-os/{codigoAcompanhamento}` | AcompanhamentoOS | Critério 3 — o cliente consulta o andamento da própria OS |
| `POST` | `/api/v1/ordens-servico/{id}/ordem/resposta` | OrdensDeServico | Critério 3 — o cliente aprova ou recusa o orçamento |

São as duas rotas com `[Authorize(Policy = ApiAuthorizationPolicies.Cliente)]`, que exige `role = Cliente` — exatamente a claim que a Lambda emite. Ambas também têm rate limit de 10 req/min por IP.

> O `AcompanhamentoOSController` **não** tem `[Authorize]` no nível da classe; a proteção está só no método. Hoje isso não abre buraco, porque é a única rota do controller — mas qualquer rota nova ali nasce desprotegida por padrão. Vale mover o `[Authorize]` para a classe.

## Rotas autenticadas

Os seis controllers abaixo têm `[Authorize]` no nível da classe: **toda** rota exige JWT válido.

### Clientes — `/api/v1/clientes` (7 rotas)

| Método | Rota |
|---|---|
| `POST` | `/` |
| `GET` | `/` |
| `GET` | `/documento/{cpfCnpj}` |
| `GET` | `/{cpfCnpj}/veiculos` |
| `POST` | `/{cpfCnpj}/veiculos` |
| `PUT` | `/documento/{cpfCnpj}` |
| `DELETE` | `/documento/{cpfCnpj}` |

Critério 4, com agravante do critério 3: CPF, nome e veículos são dado pessoal.

### Ordens de Serviço — `/api/v1/ordens-servico` (23 rotas)

| Método | Rota | Grupo |
|---|---|---|
| `POST` | `/` | Criação |
| `GET` | `/` | Consulta |
| `GET` | `/{id}` | Consulta |
| `GET` | `/{id}/historico` | Consulta |
| `GET` | `/{id}/notificacoes` | Consulta |
| `GET` | `/{id}/movimentacoes-estoque` | Consulta |
| `GET` | `/monitoramento` | Consulta |
| `GET` | `/{id}/monitoramento` | Consulta |
| `GET` | `/{id}/estimativa-tempo-servico` | Consulta |
| `PATCH` | `/{id}/iniciar-diagnostico` | Fluxo |
| `PATCH` | `/{id}/finalizar-diagnostico` | Fluxo |
| `PATCH` | `/{id}/aprovar` | Fluxo |
| `PATCH` | `/{id}/liberar-execucao` | Fluxo |
| `PATCH` | `/{id}/finalizar` | Fluxo |
| `PATCH` | `/{id}/registrar-pagamento` | Fluxo |
| `PATCH` | `/{id}/entregar` | Fluxo |
| `PATCH` | `/{numero}/avancar-status` | Fluxo |
| `PATCH` | `/{id}/cancelar` | Fluxo |
| `POST` | `/{id}/servicos` | Orçamento |
| `DELETE` | `/{id}/servicos/{servicoId}` | Orçamento |
| `POST` | `/{id}/pecas` | Orçamento |
| `DELETE` | `/{id}/pecas/{pecaId}` | Orçamento |

A 23ª rota do controller é `POST /{id}/ordem/resposta`, classificada como **Cliente** acima.

### Veículos — `/api/v1/veiculos` (7 rotas)

| Método | Rota |
|---|---|
| `POST` | `/` |
| `GET` | `/` |
| `GET` | `/{id}` |
| `GET` | `/placa/{placa}` |
| `GET` | `/cliente/{clienteId}` |
| `PUT` | `/{id}` |
| `DELETE` | `/{id}` |

### Peças — `/api/v1/pecas` (5 rotas)

| Método | Rota |
|---|---|
| `POST` | `/` |
| `GET` | `/` |
| `GET` | `/{id}` |
| `PUT` | `/{id}` |
| `DELETE` | `/{id}` |

### Serviços — `/api/v1/servicos` (5 rotas)

| Método | Rota |
|---|---|
| `POST` | `/` |
| `GET` | `/` |
| `GET` | `/{id}` |
| `PUT` | `/{id}` |
| `DELETE` | `/{id}` |

### Pedidos de Compra — `/api/v1/pedidos-compra` (4 rotas)

| Método | Rota |
|---|---|
| `POST` | `/` |
| `GET` | `/` |
| `GET` | `/ordem/{ordemDeServicoId}` |
| `PATCH` | `/{id}/receber` |

## Endpoints fora dos controllers

| Rota | Nível | Decisão |
|---|---|---|
| `/health` | **Interna** | Critério 1 — o `ResponseWriter` devolve `name`, `status`, `description` e `duration` de cada check, incluindo o do banco. `description` pode carregar mensagem de exceção do EF/Npgsql. **Não rotear pelo gateway** |
| `/health/live` | **Interna** | Liveness probe do Kubernetes. Não precisa sair da VPC |
| `/health/ready` | **Interna** | Readiness probe. Idem |
| `/swagger` | — | Já protegido: `UseApiSwagger` retorna cedo fora de `Development`. Nenhuma ação necessária |

O monitor de uptime da issue [#72](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/72) (F3-38) precisa de um alvo externo. Recomendação: expor **apenas** `/health/live`, que não devolve corpo com detalhe, e manter `/health` interno.

## Achados

Três coisas que apareceram no levantamento e afetam o F3-08 e o F3-09.

### 1. O rate limit deixa de funcionar atrás do gateway

A policy `public` particiona por `httpContext.Connection.RemoteIpAddress`, e o projeto **não configura `UseForwardedHeaders`**. Passando por API Gateway e load balancer, `RemoteIpAddress` vira o IP do proxy — igual para todo mundo.

O efeito não é o limite sumir: é ele virar **global**. As três rotas limitadas passariam a compartilhar 10 requisições por minuto entre todos os clientes somados, e o primeiro a saturar derruba os demais com `429`.

Duas saídas, e vale decidir junto com a issue [#42](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/42):

- configurar `ForwardedHeaders` na API, lendo `X-Forwarded-For`;
- ou mover o rate limit para o gateway, que já vê o IP real, e remover o da aplicação.

### 2. `[Authorize]` sem distinção de papel em 50 rotas

Os seis controllers de gestão exigem apenas **token válido**, sem checar papel. Como a Lambda emite tokens com `role = Cliente`, um token de cliente satisfaz `[Authorize]` em todas elas.

Na prática, um cliente autenticado por CPF passa na autorização de `DELETE /api/v1/pecas/{id}` e `DELETE /api/v1/servicos/{id}` — gestão de catálogo.

Isso não é regressão desta fase: as rotas já eram assim. Mas até agora o único emissor de token era o login administrativo. **Ao ligar a Lambda, a superfície muda**, e passa a existir um caminho real de cliente para operações de gestão.

Correção proporcional ao escopo da fase: criar a policy `Gestao` exigindo um papel administrativo e aplicá-la aos seis controllers, mantendo `Cliente` nas duas rotas do cliente. Não cabe no F3-10, que é levantamento — mas precisa de issue própria antes da entrega.

### 3. Cadastro de cliente tem dependência circular

`POST /api/v1/clientes` exige JWT. O token de cliente sai da Lambda, que só o emite para CPF **já existente e ativo** na tabela `Cliente`. Um cliente novo não consegue se cadastrar nem se autenticar.

Isso está correto se o cadastro for sempre feito por um atendente — o que é plausível para uma oficina. Só não está registrado em lugar nenhum, e depende do papel administrativo do achado 2. Vale uma frase explícita na documentação de negócio.

## O que o gateway roteia

Resumo operacional para a issue [#42](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/42):

| Rota no gateway | Destino | Authorizer |
|---|---|---|
| `POST /auth` | Lambda `tc-grupo160-auth-*` | Não |
| `POST /api/v1/auth/login` | API no EKS, via VPC Link | Não |
| `/api/v1/{proxy+}` | API no EKS, via VPC Link | **Sim** |
| `/health/live` | API no EKS, via VPC Link | Não — só se o monitor de uptime exigir alvo externo |

`/health` e `/health/ready` **não** entram no gateway.

## Referências

- [RFC-0002](rfcs/0002-autenticacao-por-cpf-e-api-gateway.md) — mecanismo de autenticação e escolha do gateway
- [API_REFERENCE.md](API_REFERENCE.md) — payloads e respostas de cada rota
- `src/API/Authorization/ApiAuthorizationPolicies.cs` — definição da policy `Cliente`
- `src/API/Bootstrap/ApiAuthenticationBootstrap.cs` — validação do JWT
- `src/API/Bootstrap/ApiServicesBootstrap.cs` — registro da policy e do rate limiter
