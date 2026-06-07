# Evolucao para Clean Architecture com CQRS sem MediatR

Este documento resume uma proposta de evolucao arquitetural para aproximar o projeto de Clean Architecture usando CQRS sem dependencia da biblioteca MediatR.

CQRS significa Command Query Responsibility Segregation. No contexto deste projeto, a ideia e separar operacoes que alteram estado, chamadas de Commands, de operacoes que apenas consultam dados, chamadas de Queries.

A proposta usa handlers explicitos e contratos proprios da camada Application. Opcionalmente, a API pode usar dispatchers internos (`ICommandDispatcher` e `IQueryDispatcher`) para nao depender diretamente de muitos handlers concretos.

Esta proposta nao exige banco separado, mensageria ou event sourcing. A recomendacao inicial e aplicar CQRS simples usando a mesma API, o mesmo PostgreSQL e o mesmo Entity Framework Core, mas com casos de uso separados por responsabilidade.

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
      Messaging/
      Repositories/
      ReadModels/
      Security/
      Time/
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
    Dispatchers/
    Decorators/
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

Na Application ficam commands, queries, handlers, validators, dispatchers, decorators e abstracoes:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CriarClienteCommandHandler>();
        services.AddScoped<ObterClientePorDocumentoQueryHandler>();

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
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

## 2. Definir contratos proprios de CQRS

Sem MediatR, a Application deve declarar contratos proprios para commands, queries e handlers.

```csharp
public interface ICommand<TResult>
{
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQuery<TResult>
{
}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
```

Local sugerido:

```text
Application/Abstractions/Messaging/
```

## 3. Separar Commands e Queries

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

## 4. Padronizar Request, Command, Query, Result e Response

O fluxo de clientes ja segue parcialmente este caminho:

```text
CriarClienteRequest -> CriarClienteCommand -> ClienteResult -> ClienteResponse
```

Com CQRS sem MediatR, o fluxo completo pode seguir um dos dois modelos:

```text
Request da API
  -> Command ou Query da Application
  -> Handler explicito
  -> Result da Application
  -> Response da API
```

Ou, com dispatcher proprio:

```text
Request da API
  -> Command ou Query da Application
  -> ICommandDispatcher ou IQueryDispatcher
  -> Handler da Application
  -> Result da Application
  -> Response da API
```

Esse padrao deve ser replicado em `Veiculos`, `Servicos`, `Pecas`, `OrdensDeServico` e `PedidosCompra`.

## 5. Exemplo de Command

Command representa uma intencao de alterar estado.

```csharp
public sealed class CriarClienteCommand : ICommand<ClienteResult>
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
    : ICommandHandler<CriarClienteCommand, ClienteResult>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IClock _clock;
    private readonly IValidator<CriarClienteCommand> _validator;

    public CriarClienteCommandHandler(
        IClienteRepository clienteRepository,
        IClock clock,
        IValidator<CriarClienteCommand> validator)
    {
        _clienteRepository = clienteRepository;
        _clock = clock;
        _validator = validator;
    }

    public async Task<ClienteResult> Handle(
        CriarClienteCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ServiceValidationException(
                string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage)));
        }

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

## 6. Exemplo de Query

Query representa uma consulta. Ela nao deve alterar estado.

```csharp
public sealed class ObterClientePorDocumentoQuery : IQuery<ClienteResult>
{
    public string CpfCnpj { get; init; } = null!;
}
```

Handler:

```csharp
public sealed class ObterClientePorDocumentoQueryHandler
    : IQueryHandler<ObterClientePorDocumentoQuery, ClienteResult>
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

## 7. Controller usando handlers diretamente

Esta e a alternativa mais simples. O controller depende dos handlers necessarios.

```csharp
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly CriarClienteCommandHandler _criarClienteHandler;
    private readonly ObterClientePorDocumentoQueryHandler _obterClientePorDocumentoHandler;

    public ClientesController(
        CriarClienteCommandHandler criarClienteHandler,
        ObterClientePorDocumentoQueryHandler obterClientePorDocumentoHandler)
    {
        _criarClienteHandler = criarClienteHandler;
        _obterClientePorDocumentoHandler = obterClientePorDocumentoHandler;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar(
        [FromBody] CriarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _criarClienteHandler.Handle(
            request.ToCommand(),
            cancellationToken);

        var response = result.ToResponse();

        return CreatedAtAction(
            nameof(ObterPorDocumento),
            new { cpfCnpj = response.CpfCnpj },
            response);
    }
}
```

## 8. Controller usando dispatcher proprio

Esta e a alternativa mais limpa para evitar muitos handlers concretos nos controllers.

```csharp
public interface ICommandDispatcher
{
    Task<TResult> Dispatch<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResult>;
}

public interface IQueryDispatcher
{
    Task<TResult> Dispatch<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>;
}
```

Exemplo de uso no controller:

```csharp
public class ClientesController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public ClientesController(
        ICommandDispatcher commandDispatcher,
        IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar(
        [FromBody] CriarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commandDispatcher.Dispatch<CriarClienteCommand, ClienteResult>(
            request.ToCommand(),
            cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorDocumento),
            new { cpfCnpj = result.CpfCnpj },
            result.ToResponse());
    }
}
```

## 9. Repositorios como abstracoes da Application

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

## 10. Queries podem usar repositorios de leitura

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

## 11. Commands usam o dominio

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

## 12. Remover dependencia ASP.NET da Application

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

## 13. Extrair geracao de JWT

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
    : ICommandHandler<LoginCommand, LoginResult>
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

## 14. Validacao com CQRS sem pipeline externo

Sem MediatR, ha duas alternativas simples:

```text
1. Validacao explicita dentro do handler.
2. Decorators proprios para ICommandHandler e IQueryHandler.
```

Para inicio, a validacao explicita e mais simples. Quando o padrao estabilizar, decorators podem reduzir repeticao.

Exemplo de validator:

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

Regras de negocio continuam no Domain:

```csharp
if (Status != StatusOrdemDeServico.EmDiagnostico)
{
    throw new InvalidOperationException(
        "So e possivel adicionar servicos durante o diagnostico.");
}
```

## 15. Decorators para logging, validacao e transacao

Sem MediatR, recursos equivalentes a pipeline behaviors podem ser implementados com decorators.

Exemplo conceitual:

```csharp
public sealed class TransactionCommandHandlerDecorator<TCommand, TResult>
    : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> _inner;
    private readonly ITransactionManager _transactionManager;

    public async Task<TResult> Handle(
        TCommand command,
        CancellationToken cancellationToken)
    {
        return await _transactionManager.ExecuteAsync(
            () => _inner.Handle(command, cancellationToken),
            cancellationToken);
    }
}
```

Caso o registro generico de decorators fique complexo no DI padrao, use validacao explicita e transacoes explicitas nos primeiros handlers.

## 16. Fortalecer value objects

O projeto ja possui value objects como `Documento`, `Telefone` e `PlacaVeiculo`.

A evolucao e fazer as entidades usarem esses tipos internamente sempre que possivel:

```csharp
public Documento Documento { get; private set; }
public Telefone Telefone { get; private set; }
```

Essa mudanca exige mapeamento adequado no EF Core, usando owned types ou conversions.

## 17. Encapsular entidades

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

## 18. Padronizar erros com ProblemDetails

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

## 19. Separar mapeamentos do DbContext

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

## 20. Estrutura sugerida para Ordens de Servico

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

## 21. Quando usar CQRS simples ou completo

Para este projeto, a recomendacao inicial e CQRS simples:

```text
Mesmo banco.
Mesmo DbContext.
Sem mensageria obrigatoria.
Sem banco de leitura separado.
Separacao entre Commands, Queries e Handlers.
Controllers chamando handlers diretamente ou dispatchers proprios.
```

CQRS completo com projections, eventos, mensageria e banco separado so faria sentido com necessidade real de escala, alto volume de leitura, relatorios pesados ou integracoes assincronas.

## Ordem recomendada de implementacao

1. Criar `AddApplication()` e `AddInfrastructure()`.
2. Criar contratos `ICommand`, `ICommandHandler`, `IQuery` e `IQueryHandler`.
3. Definir se controllers chamarao handlers diretamente ou dispatchers proprios.
4. Separar mappings EF com `IEntityTypeConfiguration<T>`.
5. Escolher um modulo piloto, preferencialmente `Clientes`.
6. Converter `Clientes` para CQRS sem MediatR.
7. Aplicar o mesmo padrao em `Veiculos`.
8. Extrair JWT para `ITokenGenerator`.
9. Remover dependencias ASP.NET da Application.
10. Avaliar repositories de leitura para consultas mais pesadas.
11. Converter `OrdensDeServico` por partes, priorizando commands de alteracao de status.
12. Manter o dominio encapsulado e sem dependencia de infraestrutura.

## Resultado esperado

Com essa evolucao, o projeto passa a ter:

- controllers mais simples;
- casos de uso menores e mais testaveis;
- separacao clara entre leitura e escrita;
- menos dependencia de bibliotecas externas;
- possibilidade de decorators proprios para validacao, logging e transacao;
- menos acoplamento entre API e Application;
- dominio mais protegido;
- Infrastructure isolada como detalhe tecnico;
- estrutura mais aderente a Clean Architecture sem complexidade desnecessaria.
