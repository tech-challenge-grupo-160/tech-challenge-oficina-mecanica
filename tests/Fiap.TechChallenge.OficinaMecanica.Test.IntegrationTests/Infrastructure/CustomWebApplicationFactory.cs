using System.Security.Claims;
using System.Text.Encodings.Web;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const int PessoaFisicaClienteId = 1;
    public const int PessoaJuridicaClienteId = 2;
    public const int VeiculoExistenteId = 1;
    public const int SegundoVeiculoExistenteId = 2;
    public const int ServicoExistenteId = 1000;
    public const int PecaExistenteId = 1000;
    public const string UsuarioAutenticadoId = "integration-test-user-id";
    public const string UsuarioAutenticadoNome = "integration-test-user";
    private readonly PostgreSqlContainer _postgresContainer = CreatePostgreSqlContainer();

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();

        context.ChangeTracker.Clear();
        context.Database.ExecuteSqlRaw("""
            TRUNCATE TABLE
                "MovimentacaoEstoque",
                "PedidoCompra",
                "NotificacaoCliente",
                "OrdemServicoHistorico",
                "OrdemServicoItemPeca",
                "OrdemServicoItemServico",
                "OrdemServico",
                "Peca",
                "Servico",
                "Veiculo",
                "Cliente",
                "Usuario"
            RESTART IDENTITY CASCADE;
            """);

        SeedData(context);
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        using var client = CreateClient();
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();

        await context.Database.MigrateAsync();
        ResetDatabase();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        await _postgresContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:SecretKey"] = "TestSecretKey_Should_Be_Long_Enough_123",
                ["ConnectionStrings:DefaultConnection"] = GetTestConnectionString()
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<OficinaDbContext>>();
            services.RemoveAll<OficinaDbContext>();
            services.AddDbContext<OficinaDbContext>(options =>
                options.UseNpgsql(GetTestConnectionString()));

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

        });
    }

    private static void SeedData(OficinaDbContext context)
    {
        context.OrdemDeServicoPecas.RemoveRange(context.OrdemDeServicoPecas);
        context.OrdemDeServicoServicos.RemoveRange(context.OrdemDeServicoServicos);
        context.OrdemServicoHistoricos.RemoveRange(context.OrdemServicoHistoricos);
        context.MovimentacoesEstoque.RemoveRange(context.MovimentacoesEstoque);
        context.PedidosCompra.RemoveRange(context.PedidosCompra);
        context.OrdensDeServico.RemoveRange(context.OrdensDeServico);
        context.Pecas.RemoveRange(context.Pecas);
        context.Servicos.RemoveRange(context.Servicos);
        context.Veiculos.RemoveRange(context.Veiculos);
        context.Clientes.RemoveRange(context.Clientes);
        context.SaveChanges();

        AddWithId(
            context,
            Cliente.Criar(
                "Vanessa Luna Duarte",
                Documento.Parse("476.548.668-01"),
                Telefone.Parse("15984608796"),
                Email.Parse("vanessa_luna_duarte@maissaude.adm.br"),
                DateTimeHelper.UTCBrazilNow()),
            PessoaFisicaClienteId);

        AddWithId(
            context,
            Cliente.Criar(
                "Betina e Fernanda Contabil Ltda",
                Documento.Parse("60.617.051/0001-99"),
                Telefone.Parse("16985344781"),
                Email.Parse("ouvidoria@betinaefernandacontabilltda.com.br"),
                DateTimeHelper.UTCBrazilNow()),
            PessoaJuridicaClienteId);

        AddWithId(
            context,
            Veiculo.Criar(PlacaVeiculo.Parse("BRA2E19"), "Volkswagen", "Gol", 2020, PessoaFisicaClienteId),
            VeiculoExistenteId);

        AddWithId(
            context,
            Veiculo.Criar(PlacaVeiculo.Parse("XYZ9A88"), "Fiat", "Argo", 2022, PessoaJuridicaClienteId),
            SegundoVeiculoExistenteId);

        AddWithId(
            context,
            Servico.Criar("Alinhamento", "Servico de alinhamento", 150m, 30),
            ServicoExistenteId);

        AddWithId(
            context,
            Peca.Criar("Pastilha de Freio", "Cobreq", "N-1234", 45m, 100),
            PecaExistenteId);

        context.SaveChanges();
        context.Database.ExecuteSqlRaw("""
            SELECT setval(pg_get_serial_sequence('"Cliente"', 'Id'), MAX("Id"), true) FROM "Cliente";
            SELECT setval(pg_get_serial_sequence('"Veiculo"', 'Id'), MAX("Id"), true) FROM "Veiculo";
            SELECT setval(pg_get_serial_sequence('"Servico"', 'Id'), MAX("Id"), true) FROM "Servico";
            SELECT setval(pg_get_serial_sequence('"Peca"', 'Id'), MAX("Id"), true) FROM "Peca";
            """);
    }

    private static void AddWithId<TEntity>(OficinaDbContext context, TEntity entity, int id)
        where TEntity : class
    {
        context.Add(entity);
        context.Entry(entity).Property("Id").CurrentValue = id;
    }

    private static PostgreSqlContainer CreatePostgreSqlContainer()
    {
        return new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("oficina_mecanica_tests")
            .WithUsername("postgres")
            .WithPassword(Guid.NewGuid().ToString("N"))
            .Build();
    }

    private string GetTestConnectionString()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(
            _postgresContainer.GetConnectionString())
        {
            Host = "127.0.0.1"
        };

        return connectionString.ConnectionString;
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, CustomWebApplicationFactory.UsuarioAutenticadoId),
            new Claim(ClaimTypes.Name, CustomWebApplicationFactory.UsuarioAutenticadoNome),
            new Claim(ClaimTypes.Role, "Administrador")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
