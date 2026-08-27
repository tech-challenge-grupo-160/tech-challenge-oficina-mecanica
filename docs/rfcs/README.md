# Request for Comments

Propostas técnicas **em discussão**. Uma RFC existe para o grupo debater antes de decidir — não para anunciar uma decisão já tomada.

## Índice

| RFC | Título | Status | Issue |
|---|---|---|---|
| [0001](0001-escolha-da-nuvem.md) | Escolha da nuvem — AWS | Em revisão | [#56](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/56) |
| [0002](0002-autenticacao-por-cpf-e-api-gateway.md) | Estratégia de autenticação por CPF e escolha do API Gateway | Rascunho | [#35](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/35) |
| — | Escolha da ferramenta de observabilidade | Pendente | [#66](https://github.com/tech-challenge-grupo-160/tech-challenge-oficina-mecanica/issues/66) |

A pendente é da Sprint 1 e **bloqueia o épico de observabilidade inteiro**. Enquanto não for fechada, o trabalho que depende dela avança sobre premissas, não sobre decisões.

> A RFC-0002 **corrige** um ponto da RFC-0001, que registrou *"API Gateway (REST API, com JWT authorizer)"* — combinação que não existe na AWS. Ao aceitar a 0002, ajustar a tabela de serviços da 0001.

## Fluxo

1. Abrir a RFC como Pull Request, com status `Rascunho`
2. Definir prazo para comentários
3. Discutir **no PR**, não em conversa paralela — o registro é parte do entregável
4. Ao consenso, mudar o status para `Aceita` e mergear
5. Se a decisão for permanente, escrever a ADR correspondente em [`docs/adrs/`](../adrs/)

## Convenções

- Numeração sequencial de quatro dígitos, atribuída ao abrir o PR
- Nome do arquivo: `NNNN-titulo-em-kebab-case.md`
- Use o [TEMPLATE.md](TEMPLATE.md)
- Uma RFC rejeitada **permanece no repositório**, com status `Rejeitada`. Saber o que foi descartado e por quê tem tanto valor quanto saber o que foi escolhido.
