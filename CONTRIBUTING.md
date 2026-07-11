# Guia de Contribuicao

## Branches

O projeto usa duas branches permanentes:

- `master` — producao, recebe merges via PR
- `develop` — integracao, base para novas branches

Crie branches a partir de `develop` seguindo o padrao:

```
tipo/AAAA-MM-DD-descricao
```

Tipos validos: `feature`, `fix`, `hotfix`, `refactor`, `docs`, `tests`, `infra`.

Exemplos:

```
feature/2026-07-11-adiciona-rate-limiting
hotfix/2026-07-11-corrige-transicao-status
docs/2026-07-11-criacao-doc-contributing
```

## Commits

Siga [Conventional Commits](https://www.conventionalcommits.org/):

```
tipo: descricao curta em portugues
```

Tipos: `feat`, `fix`, `refactor`, `tests`, `docs`, `ci`, `infra`.

Exemplos:

```
feat: busca em lote por IDs em servicos e pecas
fix: corrige eventos de remocao de servico e peca na OS
refactor: monitoramento de ordens de servico
docs: adiciona documentacao de uso do Postman para a API
ci: simplifica workflows e preparar CD local self-hosted
```

## Pull Requests

1. Abra o PR de sua branch para `develop`.
2. Preencha o template de PR (resumo, tipo de mudanca, camadas afetadas, checklist).
3. Aguarde aprovacao de ao menos 1 revisor antes do merge.
4. Use squash merge para manter o historico limpo.

## Arquitetura e padroes de codigo

O projeto segue Clean Architecture com CQRS:

```
API -> Application -> Domain
                  \-> Infrastructure
```

- **Controllers** recebem requests, delegam ao MediatR e retornam responses.
- **Commands/Queries** representam intencoes; cada um tem um Handler e um Validator (FluentValidation).
- **Handlers** orquestram logica de aplicacao via repositorios e domain services.
- **Entidades** encapsulam regras de negocio com metodos de dominio (nao sao anemicas).
- **Repositorios** sao abstraidos por interfaces na Application e implementados na Infrastructure.

### Convencoes

- Nao use `async void`. Todos os metodos async retornam `Task` ou `Task<T>`.
- Valide inputs com FluentValidation na camada Application (via `ValidationBehavior`).
- Excecoes de dominio: `ServiceNotFoundException` (404), `ServiceValidationException` (400), `ServiceUnauthorizedException` (401).
- Mocks nos testes unitarios usam `MockBehavior.Strict` — configure explicitamente cada metodo chamado.
- Nomeie testes no padrao `MetodoAsync_DeveComportamentoEsperado`.

## Testes

Rode todos os testes antes de abrir o PR:

```bash
dotnet test
```

- **Testes unitarios**: `tests/Fiap.TechChallenge.OficinaMecanica.Test.UnitTests/`
- **Testes de integracao**: `tests/Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests/`

Testes de integracao dependem do PostgreSQL via Docker (Testcontainers).

## Ambiente local

```bash
docker-compose up --build
```

Consulte [docs/SETUP.md](docs/SETUP.md) para execucao sem Docker.

## Documentacao

Ao alterar endpoints ou comportamento da API, atualize:

- [docs/API_REFERENCE.md](docs/API_REFERENCE.md) — referencia de endpoints e erros
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — diagramas de arquitetura

## Grupo

Tech Challenge FIAP — Grupo 160.
