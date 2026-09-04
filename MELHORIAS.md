# Sugestoes de Melhoria

## Melhorias de codigo

### 2. Duplicacao excessiva na entidade OrdemDeServico

**Severidade:** Media  
**Arquivo:** `src/Domain/Entities/OrdemDeServico.cs`

Os metodos `AdicionarServico` / `AdicionarServicoNaAberturaComEvento` e `AdicionarPeca` / `AdicionarPecaNaAberturaComEvento` sao quase identicos, diferindo apenas na validacao de status. Isso poderia ser consolidado com um parametro ou metodo privado que aceita os status validos.

---

### 3. Handlers com boilerplate repetitivo

**Severidade:** Baixa  
**Arquivos:** `src/Application/Handlers/OrdensDeServico/*.cs`

Todos os handlers seguem o mesmo padrao: log start, try, buscar ordem, validar, operar, log end, catch log error, throw. O bloco try/catch que apenas faz log e re-throw nao agrega valor — o logging de excecoes nao tratadas pode ser centralizado em um middleware ou behavior do MediatR.

---

---

### 5. Falta de base entity / auditoria

**Severidade:** Media  
**Arquivos:** `src/Domain/Entities/*.cs`

Nao ha uma classe base para entidades com campos comuns (`Id`, `CriadoEm`, `AtualizadoEm`). Cada entidade repete `Id` como `int` independentemente. Um `BaseEntity` facilitaria auditoria e consistencia.

---

### 9. Secrets no Terraform em plain text

**Severidade:** ~~Alta~~ Baixa — resolvido para a nuvem em 04/09  
**Arquivo:** `local/main.tf` no [tech-challenge-infra-k8s](https://github.com/tech-challenge-grupo-160/tech-challenge-infra-k8s)

Os secrets do PostgreSQL e JWT eram passados como variaveis Terraform comuns e ficavam no state em plain text.

Na nuvem isso nao existe mais: a chave do JWT e a credencial do banco vivem no AWS Secrets Manager, e o deploy monta um Secret do Kubernetes a partir delas a cada execucao, sem nunca versionar valor. Ver RFC-0002 e `secrets.tf` no repositorio de infraestrutura.

O que sobrou e o ambiente kind local, que foi para `local/` no `infra-k8s` na issue #65. Ali os valores sao de desenvolvimento (`dev-secret-key-minimo-32-caracteres-ok`), o state e local e o cluster e descartavel — continua nao sendo bonito, mas nao e mais risco.

---

### 11. `SomarQuantidade` sobrescreve o preco

**Severidade:** Media  
**Arquivo:** `src/Domain/Entities/OrdemDeServico.cs` (linhas 627-636)

`OrdemDeServicoPeca.SomarQuantidade` recebe `preco` e sobrescreve o preco anterior. Se o preco da peca mudou entre as adicoes, o preco anterior e perdido sem rastreabilidade.

---

## Melhorias de documentacao

## Itens implementados

| Item | Descricao | Arquivo alterado/criado |
|---|---|---|
| 13 | Link quebrado no `docs/README.md` | `docs/README.md` |
| 14 | API_REFERENCE.md desatualizado em relacao aos controllers | `docs/API_REFERENCE.md` |
| 16 | Falta de documentacao de codigos de erro por endpoint | `docs/API_REFERENCE.md` |
| 17 | README principal com link incompleto para execucao local | `README.md` |
| 20 | Ausencia de templates de PR e issue no GitHub | `.github/PULL_REQUEST_TEMPLATE.md`, `.github/ISSUE_TEMPLATE/` |
| 21 | Documentacao de infraestrutura com formatacao inconsistente | `docs/INFRAESTRUTURA.md` |
| 22 | Falta de diagrama visual de arquitetura | `docs/ARCHITECTURE.md` |
| 23 | Documentacao de Postman sem instrucoes de uso | `docs/postman/README.md`, `docs/SETUP.md` |
| 15 | Documentacao de erros por endpoint na API_REFERENCE | `docs/API_REFERENCE.md` |
| 1 | Bug: Tipo de evento errado ao remover itens | `src/Domain/Enums/TipoEventoOrdemServico.cs`, `src/Domain/Entities/OrdemDeServico.cs` |
| 4 | Consultas N+1 na criacao da OS | `IServicoRepository.cs`, `IPecaRepository.cs`, `ServicoRepository.cs`, `PecaRepository.cs`, `CriarOrdemDeServicoCommandHandler.cs` |
| 6 | `ObterTodasAsync` sem paginacao | `IOrdemDeServicoRepository.cs`, `OrdemDeServicoRepository.cs`, `ObterResumoMonitoramentoOrdensDeServicoQueryHandler.cs` |
| 8 | Validacao de transicao de status incompleta | `src/Domain/Entities/OrdemDeServico.cs` |
| 10 | Falta de idempotencia em operacoes | `src/Domain/Entities/OrdemDeServico.cs` |
| 18 | Ausencia de CONTRIBUTING.md | `CONTRIBUTING.md` |
| 19 | Ausencia de CHANGELOG.md | `CHANGELOG.md` |
| 24 | CLEAN_ARCHITECTURE_EVOLUTION.md referencia estado futuro sem data | `docs/CLEAN_ARCHITECTURE_EVOLUTION.md` |
| 12 | Sem rate limiting no endpoint publico | `ApiServicesBootstrap.cs`, `ApiPipelineBootstrap.cs`, `AcompanhamentoOSController.cs`, `OrdensDeServicoController.cs`, `AuthController.cs` |
| 7 | Filtro fixo no `AplicarFiltros` | Regra de negocio confirmada — sem alteracao necessaria |
