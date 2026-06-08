# Evolucao para Clean Architecture com CQRS e MediatR

Este documento resume uma proposta de evolucao arquitetural para aproximar o projeto de Clean Architecture usando CQRS com MediatR.

CQRS significa Command Query Responsibility Segregation. No contexto deste projeto, a ideia e separar operacoes que alteram estado, chamadas de Commands, de operacoes que apenas consultam dados, chamadas de Queries.

MediatR entra como mediador entre a API e os casos de uso da Application. Em vez de controllers chamarem services grandes ou handlers concretos, eles enviam Commands e Queries via `IMediator`.

Esta proposta nao exige banco separado, mensageria ou event sourcing. A recomendacao inicial e aplicar CQRS com MediatR usando a mesma API, o mesmo PostgreSQL e o mesmo Entity Framework Core, mas com casos de uso separados por responsabilidade.

## Estrutura proposta

```text
src/
  API/
    Controllers/
    Requests/
    Responses/
    Mappers/
    Validators/

  Application/
    Abstractions/
      Repositories/
      ReadModels/
      Security/
      Time/
    Behaviors/
    Commands/
      Clientes/
      Veiculos/
      Servicos/
      Pecas/
      OrdensDeServico/
      PedidosCompra/
      Auth/
    Queries/
      Clientes/
      Veiculos/
      Servicos/
      Pecas/
      OrdensDeServico/
      PedidosCompra/
      AcompanhamentoOS/
    Results/
    Validators/

  Domain/
    Entities/
    Enums/
    ValueObjects/

  Infrastructure/
    Data/
    Data/Configurations/
    Repositories/
    ReadModels/
    Security/
    Time/
```

## 1. Limpar o composition root

Hoje o `Program.cs` concentra muitos registros: DbContext, repositories, application services, clock, autenticacao, Swagger e health checks.

A evolucao recomendada e criar extensoes por camada:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
```

Na Application ficam handlers do MediatR, validators, pipeline behaviors e abstracoes:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
```

Pacotes esperados na Application:

```text
MediatR
FluentValidation
FluentValidation.DependencyInjectionExtensions
```

Na Infrastructure ficam EF Core, repositories e implementacoes tecnicas:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<OficinaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IClock, BrazilClock>();

        return services;
    }
}
```

## 2. Separar Commands e Queries

Hoje services como `ClienteApplicationService` misturam operacoes de escrita e leitura.

Exemplo atual:

```text
CriarClienteAsync
ObterClienteAsync
ObterClientePorCpfCnpjAsync
ListarClientesAsync
AtualizarClientePorCpfCnpjAsync
DeletarClientePorCpfCnpjAsync
```

Com CQRS, separar:

```text
Commands:
- CriarCliente
- AtualizarCliente
- DeletarCliente

Queries:
- ObterCliente
- ObterClientePorDocumento
- ListarClientes
```

Estrutura sugerida:

```text
Application/
  Commands/
    Clientes/
      CriarCliente/
        CriarClienteCommand.cs
        CriarClienteCommandHandler.cs
      AtualizarCliente/
        AtualizarClienteCommand.cs
        AtualizarClienteCommandHandler.cs
      DeletarCliente/
        DeletarClienteCommand.cs
        DeletarClienteCommandHandler.cs

  Queries/
    Clientes/
      ObterClientePorDocumento/
        ObterClientePorDocumentoQuery.cs
        ObterClientePorDocumentoQueryHandler.cs
      ListarClientes/
        ListarClientesQuery.cs
        ListarClientesQueryHandler.cs
```

## 3. Padronizar Request, Command, Query, Result e Response

O fluxo de clientes ja segue parcialmente este caminho:

```text
CriarClienteRequest -> CriarClienteCommand -> ClienteResult -> ClienteResponse
```

Com CQRS e MediatR, o fluxo completo fica:

```text
Request da API
  -> Command ou Query da Application
  -> IMediator
  -> Handler da Application
  -> Result da Application
  -> Response da API
```

Exemplo:

```text
CriarClienteRequest
  -> CriarClienteCommand
  -> IMediator.Send(command)
  -> CriarClienteCommandHandler
  -> ClienteResult
  -> ClienteResponse
```

Esse padrao deve ser replicado em `Veiculos`, `Servicos`, `Pecas`, `OrdensDeServico` e `PedidosCompra`.

## 4. Exemplo de Command

Command representa uma intencao de alterar estado.

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

    public CriarClienteCommandHandler(
        IClienteRepository clienteRepository,
        IClock clock)
    {
        _clienteRepository = clienteRepository;
        _clock = clock;
    }

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

## 5. Exemplo de Query

Query representa uma consulta. Ela nao deve alterar estado.

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

    public ObterClientePorDocumentoQueryHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

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

## 6. Controller usando CQRS com MediatR

O controller deixa de depender de um service grande ou de handlers concretos. Ele depende apenas de `IMediator`.

```csharp
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar(
        [FromBody] CriarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            request.ToCommand(),
            cancellationToken);

        var response = result.ToResponse();

        return CreatedAtAction(
            nameof(ObterPorDocumento),
            new { cpfCnpj = response.CpfCnpj },
            response);
    }

    [HttpGet("documento/{cpfCnpj}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorDocumento(
        string cpfCnpj,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ObterClientePorDocumentoQuery { CpfCnpj = cpfCnpj },
            cancellationToken);

        return Ok(result.ToResponse());
    }
}
```

## 7. Repositorios como abstracoes da Application

Atualmente os contratos estao em `src/Domain/Repositories`.

Uma alternativa mais alinhada a Clean Architecture e mover esses contratos para:

```text
Application/Abstractions/Repositories/
```

Exemplo:

```csharp
namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
    Task<Cliente> CriarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task<Cliente> AtualizarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task DeletarAsync(int id, CancellationToken cancellationToken);
}
```

A implementacao continua na Infrastructure:

```text
Infrastructure/Repositories/ClienteRepository.cs
```

## 8. Queries podem usar repositorios de leitura

No inicio, Commands e Queries podem usar os mesmos repositories.

Se as consultas crescerem ou precisarem de performance, crie repositories especificos de leitura:

```text
Application/Abstractions/ReadModels/IClienteReadRepository.cs
Infrastructure/ReadModels/ClienteReadRepository.cs
```

Exemplo:

```csharp
public interface IClienteReadRepository
{
    Task<ClienteResult?> ObterPorDocumentoAsync(
        string cpfCnpj,
        CancellationToken cancellationToken);
}
```

Esse tipo de repository pode retornar direto um `Result`, sem carregar o agregado completo, desde que nao altere estado.

## 9. Commands usam o dominio

Commands devem passar pelas entidades de dominio e respeitar invariantes.

Exemplo em ordem de servico:

```csharp
ordem.AdicionarServico(servico);
ordem.FinalizarDiagnostico();
ordem.LiberarExecucaoAposValidacaoEstoque();
ordem.Entregar();
```

Evite alterar estado diretamente:

```csharp
ordem.Status = StatusOrdemDeServico.Finalizada;
```

Prefira metodos de negocio:

```csharp
ordem.FinalizarServico();
```

## 10. Remover dependencia ASP.NET da Application

A camada Application deve depender de abstracoes proprias, nao de detalhes HTTP.

Exemplo:

```csharp
public interface ICurrentUser
{
    string? Id { get; }
    string? Nome { get; }
}
```

A implementacao que usa `HttpContext` deve ficar na API:

```csharp
public sealed class UsuarioAutenticadoService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public string? Id =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
```

Com isso, a Application nao precisa referenciar ASP.NET Core.

## 11. Extrair geracao de JWT

O login pode ser modelado como Command:

```text
Application/Commands/Auth/Login/
  LoginCommand.cs
  LoginCommandHandler.cs
```

A geracao concreta do JWT deve ficar fora da Application:

```csharp
public interface ITokenGenerator
{
    LoginResult Generate(Usuario usuario);
}
```

Handler:

```csharp
public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenGenerator _tokenGenerator;

    public async Task<LoginResult> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObterPorUsuarioAsync(
            command.Usuario,
            cancellationToken);

        if (usuario == null)
        {
            throw new ServiceUnauthorizedException("Usuario ou senha invalidos.");
        }

        var senhaHash = StringHelper.ToMd5Hash(command.Senha);

        if (!string.Equals(senhaHash, usuario.SenhaHash, StringComparison.OrdinalIgnoreCase))
        {
            throw new ServiceUnauthorizedException("Usuario ou senha invalidos.");
        }

        return _tokenGenerator.Generate(usuario);
    }
}
```

## 12. Validacao com CQRS e pipeline behavior

Valide entrada por Command ou Query.

```text
Application/Validators/Clientes/CriarClienteCommandValidator.cs
Application/Validators/Clientes/AtualizarClienteCommandValidator.cs
Application/Validators/Clientes/ListarClientesQueryValidator.cs
```

Exemplo:

```csharp
public sealed class CriarClienteCommandValidator : AbstractValidator<CriarClienteCommand>
{
    public CriarClienteCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CpfCnpj).NotEmpty();
        RuleFor(x => x.Telefone).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

Com MediatR, a validacao pode rodar automaticamente antes do handler usando um pipeline behavior:

```csharp
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var errors = failures
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new ServiceValidationException(
                string.Join("; ", errors.Select(error => error.ErrorMessage)));
        }

        return await next();
    }
}
```

Regras de negocio continuam no Domain:

```csharp
if (Status != StatusOrdemDeServico.EmDiagnostico)
{
    throw new InvalidOperationException(
        "So e possivel adicionar servicos durante o diagnostico.");
}
```

## 13. Pipeline behaviors para logging e transacao

Depois da validacao, o proximo passo natural e usar pipeline behaviors para centralizar cross-cutting concerns.

Exemplos:

```text
LoggingBehavior<TRequest, TResponse>
TransactionBehavior<TRequest, TResponse>
```

Uso esperado:

- `ValidationBehavior` valida commands e queries antes do handler.
- `LoggingBehavior` padroniza logs de entrada, saida e falha.
- `TransactionBehavior` abre transacao apenas para requests de escrita.

Se necessario, uma interface marcador pode ajudar a limitar transacao apenas para Commands:

```csharp
public interface ICommand<TResult> : IRequest<TResult>
{
}
```

## 14. Fortalecer value objects

O projeto ja possui value objects como `Documento`, `Telefone` e `PlacaVeiculo`.

A evolucao e fazer as entidades usarem esses tipos internamente sempre que possivel:

```csharp
public Documento Documento { get; private set; }
public Telefone Telefone { get; private set; }
```

Essa mudanca exige mapeamento adequado no EF Core, usando owned types ou conversions.

## 15. Encapsular entidades

Entidades devem expor comportamento, nao apenas dados mutaveis.

```csharp
public string Nome { get; private set; }

public void AtualizarNome(string nome)
{
    if (string.IsNullOrWhiteSpace(nome))
    {
        throw new ArgumentException("Nome e obrigatorio.");
    }

    Nome = nome.Trim();
}
```

Esse padrao deve ser aplicado gradualmente em entidades que ainda tenham setters publicos.

## 16. Padronizar erros com ProblemDetails

O `DomainExceptionFilter` pode evoluir para retornar `ProblemDetails`, padrao do ASP.NET Core.

```csharp
return new ProblemDetails
{
    Title = "Erro de validacao",
    Detail = exception.Message,
    Status = StatusCodes.Status400BadRequest,
    Instance = context.HttpContext.Request.Path
};
```

Isso melhora a interoperabilidade da API e facilita documentacao.

## 17. Separar mapeamentos do DbContext

O `OficinaDbContext` concentra muitos mapeamentos no `OnModelCreating`.

A melhoria e separar em configuracoes por entidade:

```text
Infrastructure/Data/Configurations/ClienteConfiguration.cs
Infrastructure/Data/Configurations/VeiculoConfiguration.cs
Infrastructure/Data/Configurations/OrdemDeServicoConfiguration.cs
```

No DbContext:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(OficinaDbContext).Assembly);
}
```

## 18. Estrutura sugerida para Ordens de Servico

Como ordem de servico e o agregado principal do projeto, CQRS ajuda a organizar melhor esse fluxo.

Commands:

```text
Commands/OrdensDeServico/CriarOrdemDeServico
Commands/OrdensDeServico/IniciarDiagnostico
Commands/OrdensDeServico/AdicionarServico
Commands/OrdensDeServico/AdicionarPeca
Commands/OrdensDeServico/FinalizarDiagnostico
Commands/OrdensDeServico/AprovarOrcamento
Commands/OrdensDeServico/LiberarExecucao
Commands/OrdensDeServico/FinalizarServico
Commands/OrdensDeServico/RegistrarPagamento
Commands/OrdensDeServico/Entregar
Commands/OrdensDeServico/Cancelar
```

Queries:

```text
Queries/OrdensDeServico/ObterOrdemDeServico
Queries/OrdensDeServico/ListarOrdensDeServico
Queries/OrdensDeServico/ObterHistorico
Queries/OrdensDeServico/ObterNotificacoes
Queries/OrdensDeServico/ObterMovimentacoesEstoque
Queries/OrdensDeServico/ObterMonitoramento
Queries/OrdensDeServico/ObterEstimativaTempoServico
Queries/AcompanhamentoOS/ObterAcompanhamentoPublico
```

## 19. Quando usar CQRS com MediatR simples ou completo

Para este projeto, a recomendacao inicial e CQRS com MediatR simples:

```text
Mesmo banco.
Mesmo DbContext.
Sem mensageria obrigatoria.
Sem banco de leitura separado.
Separacao entre Commands, Queries e Handlers via MediatR.
Controllers chamando apenas IMediator.
```

CQRS completo com projections, eventos, mensageria e banco separado so faria sentido com necessidade real de escala, alto volume de leitura, relatorios pesados ou integracoes assincronas.

## Ordem recomendada de implementacao

1. Criar `AddApplication()` e `AddInfrastructure()`.
2. Adicionar MediatR na Application.
3. Registrar `AddMediatR` em `AddApplication()`.
4. Criar `ValidationBehavior<TRequest, TResponse>` com FluentValidation.
5. Separar mappings EF com `IEntityTypeConfiguration<T>`.
6. Escolher um modulo piloto, preferencialmente `Clientes`.
7. Converter `Clientes` para CQRS com MediatR:
   - `CriarClienteCommand : IRequest<ClienteResult>`
   - `CriarClienteCommandHandler : IRequestHandler<CriarClienteCommand, ClienteResult>`
   - `AtualizarClienteCommand`
   - `DeletarClienteCommand`
   - `ObterClientePorDocumentoQuery`
   - `ListarClientesQuery`
8. Alterar o controller de `Clientes` para usar apenas `IMediator`.
9. Aplicar o mesmo padrao em `Veiculos`.
10. Extrair JWT para `ITokenGenerator`.
11. Remover dependencias ASP.NET da Application.
12. Avaliar repositories de leitura para consultas mais pesadas.
13. Converter `OrdensDeServico` por partes, priorizando commands de alteracao de status.
14. Manter o dominio encapsulado e sem dependencia de infraestrutura.

## Resultado esperado

Com essa evolucao, o projeto passa a ter:

- controllers mais simples;
- casos de uso menores e mais testaveis;
- separacao clara entre leitura e escrita;
- uso padronizado de `IMediator` entre API e Application;
- possibilidade de pipeline behaviors para validacao, logging e transacao;
- menos acoplamento entre API e Application;
- dominio mais protegido;
- Infrastructure isolada como detalhe tecnico;
- estrutura mais aderente a Clean Architecture sem complexidade desnecessaria.
