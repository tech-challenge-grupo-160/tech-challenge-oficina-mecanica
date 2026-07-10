# Evolucao para Clean Architecture com CQRS e MediatR

Ultima atualizacao: 2026-07-10

Este documento descreve o estado atual da migracao arquitetural do projeto. A base ja esta organizada em Clean Architecture com CQRS e MediatR: controllers enviam Commands e Queries por `IMediator`, handlers executam os casos de uso, validators rodam no pipeline do MediatR e a Infrastructure implementa detalhes tecnicos.

O objetivo daqui em diante e manter a documentacao sincronizada com o codigo e orientar a continuidade da migracao sem reintroduzir services grandes, dependencias HTTP na Application ou acesso direto a infraestrutura pelos controllers.

## Historico da migracao

| Data | Marco | PR/Commit |
|---|---|---|
| 2026-06-04 | Separacao em projetos Clean Architecture (API, Application, Domain, Infrastructure, Shared) | #47 |
| 2026-06-05 | Separacao de DTOs e interfaces de services na Application | #49, #50 |
| 2026-06-08 | Integracao MediatR 12.5.0 + ValidationBehavior com FluentValidation | #99, #100 |
| 2026-06-08 | Separacao de mapeamentos EF Core em `IEntityTypeConfiguration<T>` | #101 |
| 2026-06-08 | Migracao piloto: Clientes para CQRS com MediatR | #102 |
| 2026-06-08 | Migracao: Veiculos para CQRS | #103 |
| 2026-06-09 | Migracao: Pecas e Servicos para CQRS | #104, #105 |
| 2026-06-10 | Migracao: Pedidos de Compra para CQRS | #107 |
| 2026-06-10 | Migracao: Ordens de Servico para CQRS | #151 |
| 2026-06-11 | Extracao de geracao JWT para `ITokenGenerator` / `JwtTokenGenerator` | #152 |
| 2026-06-12 | Extracao do bootstrap da API em extensoes dedicadas | #153 |
| 2026-06-13 | Migracao: AcompanhamentoOS para CQRS | #154 |
| 2026-06-13 | Padronizacao de erros com ProblemDetails | #158 |
| 2026-06-15 | Troca de hash MD5 para BCrypt na autenticacao | #163 |
| 2026-06-15 | Remocao da dependencia `Microsoft.AspNetCore.App` da camada Application | #164 |
| 2026-06-21 | Fortalecimento de Value Objects em Cliente e Veiculo | #167 |
| 2026-07-01 | Atualizacao de testes unitarios e de integracao para handlers MediatR | #182, #184 |
| 2026-07-04 | Pipeline CI com build, testes unitarios e integracao separados + cobertura | #190, #192 |

## Estado atual da migracao

Concluido:

- [x] `Program.cs` delega registros para `AddApiServices`, `AddApplication` e `AddInfrastructure`.
- [x] Controllers dependem de `IMediator`, nao de handlers concretos.
- [x] Casos de uso estao separados em `Commands`, `Queries` e `Handlers`.
- [x] Commands e Queries implementam `IRequest<TResponse>`.
- [x] Handlers implementam `IRequestHandler<TRequest,TResponse>`.
- [x] Results ficam na Application.
- [x] Responses ficam na API.
- [x] Mappers da API convertem `Request -> Command/Query` e `Result -> Response`.
- [x] `ValidationBehavior<TRequest,TResponse>` executa validators da Application antes dos handlers.
- [x] Mapeamentos EF estao separados em `Infrastructure/Data/Configurations`.
- [x] JWT esta abstraido por `ITokenGenerator` e implementado em `JwtTokenGenerator`.
- [x] `IClock` esta abstraido na Application e implementado em `BrazilClock`.
- [x] Dependencia `Microsoft.AspNetCore.App` removida da Application (concluido em 2026-06-15, PR #164).
- [x] Value Objects fortalecidos em Cliente e Veiculo (concluido em 2026-06-21, PR #167).

Pontos ainda aceitos no estado atual:

- [ ] Contratos de repositories permanecem em `Domain/Repositories` — mover para `Application/Interfaces` e uma possibilidade futura, mas nao ha urgencia.
- [ ] Alguns servicos de apoio ainda existem em `Application/Services`, principalmente para coordenar regras de ordem de servico.
- [ ] Commands e Queries usam o mesmo banco e o mesmo `OficinaDbContext`.
- [ ] Nao ha mensageria, event sourcing ou banco de leitura separado.

## Estrutura real

```text
src/
  API/
    Bootstrap/
    Controllers/
    Filters/
    Mappers/
    ProblemDetails/
    Requests/
    Responses/
    Services/
    Validators/

  Application/
    Behaviors/
    Commands/
    Common/
    DTOs/
    Exceptions/
    Handlers/
    Interfaces/
    Mappers/
    Options/
    Queries/
    Results/
    Security/
    Services/
    Validators/

  Domain/
    Entities/
    Enums/
    Repositories/
    ValueObjects/

  Infrastructure/
    Data/
    Data/Configurations/
    Data/Seeders/
    Extensions/
    HealthChecks/
    Logging/
    Migrations/
    Repositories/
    Security/
    Time/

  Shared/
    Helpers/
    Logging/
```

## Padrao Controller -> IMediator -> Handler

Todo endpoint deve seguir este padrao:

```text
Controller
  -> recebe Request/body/route/query string
  -> cria Command ou Query usando mapper da API
  -> chama _mediator.Send(commandOrQuery, cancellationToken)
  -> recebe Result
  -> converte Result para Response
  -> retorna ActionResult
```

O controller deve conter apenas logica HTTP:

- binding de parametros;
- normalizacao simples de paginacao ou filtros;
- escolha de status HTTP;
- chamada ao mediator;
- conversao para response.

O controller nao deve:

- injetar repository;
- injetar `OficinaDbContext`;
- chamar handler concreto;
- conter regra de negocio;
- retornar `Result` da Application diretamente;
- expor entidades de dominio.

## Fluxo Request -> Command/Query -> Handler -> Result -> Response

```text
Request da API
  -> Command ou Query da Application
  -> IMediator
  -> ValidationBehavior
  -> Handler da Application
  -> Domain / Repository / Service de apoio
  -> Result da Application
  -> Response da API
```

Exemplo real do modulo de clientes:

```text
CriarClienteRequest
  -> ClienteApiMapper.ToCommand()
  -> CriarClienteCommand : IRequest<ClienteResult>
  -> IMediator.Send(command)
  -> CriarClienteCommandValidator
  -> CriarClienteCommandHandler
  -> Cliente.Criar(...)
  -> IClienteRepository.CriarAsync(...)
  -> ClienteResult
  -> ClienteApiMapper.ToResponse()
  -> ClienteResponse
```

### Request

Fica em `src/API/Requests`.

Representa o contrato HTTP de entrada. Deve ter somente os dados que o consumidor envia para a API. Pode ser validado por validators da API, como `CriarClienteRequestValidator`.

### Command

Fica em `src/Application/Commands`.

Representa uma intencao de escrita. Deve implementar `IRequest<TResult>` ou `IRequest<Unit>`.

Exemplos:

- `CriarClienteCommand`
- `AtualizarVeiculoCommand`
- `CriarPedidoCompraCommand`
- `LiberarExecucaoCommand`
- `RegistrarPagamentoCommand`

### Query

Fica em `src/Application/Queries`.

Representa uma consulta. Deve implementar `IRequest<TResult>` e nao deve alterar estado.

Exemplos:

- `ListarClientesQuery`
- `ObterVeiculoPorPlacaQuery`
- `ListarOrdensDeServicoQuery`
- `ObterAcompanhamentoOSQuery`
- `ObterMovimentacoesEstoqueOrdemDeServicoQuery`

### Handler

Fica em `src/Application/Handlers`.

Executa o caso de uso. Deve implementar `IRequestHandler<TRequest,TResult>`.

Responsabilidades:

- carregar entidades por repositories;
- aplicar value objects e regras de negocio;
- chamar metodos das entidades;
- coordenar servicos de apoio quando necessario;
- persistir alteracoes por contratos de repositorio;
- retornar Result;
- lancar excecoes previsiveis da Application quando necessario.

### Result

Fica em `src/Application/Results`.

Representa o retorno interno do caso de uso. Ele separa a Application dos contratos HTTP. Um Result pode ser convertido para uma Response pela API.

### Response

Fica em `src/API/Responses`.

Representa o contrato HTTP de saida. Deve ser pensado para consumidores externos, sem vazar entidades de dominio nem detalhes de persistencia.

## Como criar ou migrar um caso de uso

Use esta ordem para manter consistencia:

1. Criar ou ajustar o `Request` em `API/Requests`, quando o endpoint recebe body.
2. Criar ou ajustar o `Response` em `API/Responses`, quando o retorno publico muda.
3. Criar o `Command` ou `Query` em `Application/Commands` ou `Application/Queries`.
4. Criar o validator do Command/Query em `Application/Validators`.
5. Criar o `Handler` em `Application/Handlers`.
6. Usar entidades, value objects e repositories pelo handler.
7. Retornar um `Result` de `Application/Results`.
8. Criar ou atualizar o mapper da Application, quando houver conversao de entidade para Result.
9. Criar ou atualizar o mapper da API para `Request -> Command/Query` e `Result -> Response`.
10. Ajustar o controller para chamar somente `_mediator.Send(...)`.
11. Cobrir com teste unitario de handler e, quando o fluxo HTTP mudar, teste de integracao de endpoint.

## Exemplo de Command

```csharp
public sealed class CriarClienteCommand : IRequest<ClienteResult>
{
    public string Nome { get; init; } = null!;
    public string CpfCnpj { get; init; } = null!;
    public string Telefone { get; init; } = null!;
    public string Email { get; init; } = null!;
}
```

Handler:

```csharp
public sealed class CriarClienteCommandHandler
    : IRequestHandler<CriarClienteCommand, ClienteResult>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IClock _clock;

    public async Task<ClienteResult> Handle(
        CriarClienteCommand command,
        CancellationToken cancellationToken)
    {
        var documento = Documento.Parse(command.CpfCnpj);
        var telefone = Telefone.Parse(command.Telefone);

        var clienteExistente = await _clienteRepository.ObterPorCpfCnpjAsync(
            documento.Valor,
            cancellationToken);

        if (clienteExistente != null)
        {
            throw new ServiceValidationException("Cliente com este CPF/CNPJ ja existe.");
        }

        var cliente = Cliente.Criar(
            command.Nome,
            documento,
            telefone,
            command.Email,
            _clock.Now);

        var clienteCriado = await _clienteRepository.CriarAsync(cliente, cancellationToken);

        return clienteCriado.ToResult();
    }
}
```

## Exemplo de Query

```csharp
public sealed class ObterClientePorDocumentoQuery : IRequest<ClienteResult>
{
    public string CpfCnpj { get; init; } = null!;
}
```

Handler:

```csharp
public sealed class ObterClientePorDocumentoQueryHandler
    : IRequestHandler<ObterClientePorDocumentoQuery, ClienteResult>
{
    private readonly IClienteRepository _clienteRepository;

    public async Task<ClienteResult> Handle(
        ObterClientePorDocumentoQuery query,
        CancellationToken cancellationToken)
    {
        var documento = Documento.Parse(query.CpfCnpj).Valor;

        var cliente = await _clienteRepository.ObterPorCpfCnpjAsync(
            documento,
            cancellationToken);

        if (cliente == null)
        {
            throw new ServiceNotFoundException(
                $"Cliente com CPF/CNPJ {query.CpfCnpj} nao encontrado.");
        }

        return cliente.ToResult();
    }
}
```

## Validacao com pipeline behavior

`AddApplication()` registra:

```csharp
services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

O `ValidationBehavior<TRequest,TResponse>` busca todos os `IValidator<TRequest>` registrados e executa a validacao antes do handler. Quando ha erro, ele lanca `ServiceValidationException`, que a API converte em resposta HTTP padronizada.

Padrao de pastas:

```text
Application/Validators/Clientes/CriarClienteCommandValidator.cs
Application/Validators/Clientes/ListarClientesQueryValidator.cs
Application/Validators/OrdensDeServico/LiberarExecucaoCommandValidator.cs
```

Validadores de `API/Validators` continuam existindo para o contrato HTTP. Validadores de `Application/Validators` validam o caso de uso.

## Repositories e Infrastructure

No estado atual, os contratos ficam em:

```text
Domain/Repositories/
```

As implementacoes ficam em:

```text
Infrastructure/Repositories/
```

`AddInfrastructure()` registra os repositories concretos, `OficinaDbContext`, `ITransactionManager`, `IClock` e `ITokenGenerator`.

Essa estrutura e valida para a migracao atual. Se a equipe decidir mover contratos de repositorio para `Application/Interfaces` no futuro, a mudanca deve ser feita de forma planejada e atualizada nesta documentacao.

## Proximos passos

Itens pendentes, sem prazo definido. Devem ser avaliados conforme necessidade:

1. Avaliar a movimentacao dos contratos de repositorio de `Domain/Repositories` para `Application/Interfaces`.
2. Considerar CQRS completo (mensageria, banco de leitura separado) apenas se houver demanda concreta de escala ou relatorios pesados.
3. Centralizar logging de excecoes em um behavior do MediatR em vez de try/catch repetitivo nos handlers.
4. Adicionar rate limiting nos endpoints publicos (`AcompanhamentoOS`, resposta de OS).

## Regras para novas implementacoes

- Controller injeta apenas `IMediator` e dependencias estritamente HTTP.
- Command altera estado; Query nao altera estado.
- Handler nao retorna Response da API.
- Handler nao recebe Request da API.
- Entity nao recebe DTO, Request, Response ou Result.
- Infrastructure nao deve ser chamada diretamente pela API fora do composition root.
- Validacao de entrada fica em validators; invariantes ficam no dominio.
- Result e o contrato entre Application e API.
- Response e o contrato entre API e consumidor externo.

## Quando usar CQRS simples ou completo

O projeto usa CQRS simples:

```text
Mesmo banco.
Mesmo DbContext.
Sem mensageria obrigatoria.
Sem banco de leitura separado.
Separacao entre Commands, Queries e Handlers via MediatR.
Controllers chamando IMediator.
```

CQRS completo com projections, eventos, mensageria e banco separado so deve ser considerado se houver necessidade concreta de escala, alto volume de leitura, relatorios pesados ou integracoes assincronas.

## Checklist de aceite para mudancas futuras

- A documentacao de arquitetura continua refletindo a estrutura real.
- Nao ha referencias a services antigos como fluxo principal.
- Todo endpoint novo segue `Controller -> IMediator -> Handler`.
- Commands, Queries, Handlers, Results e Responses estao nas pastas corretas.
- Testes cobrem handlers novos ou alterados.
- README e docs sao atualizados quando dependencias, execucao ou estrutura mudarem.
- Este documento e atualizado quando houver mudanca na arquitetura ou nos itens pendentes.
