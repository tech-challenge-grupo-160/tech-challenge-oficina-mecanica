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

namespace Fiap.TechChallenge.OficinaMecanica.Test.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const int PessoaFisicaClienteId = 1;
    public const int PessoaJuridicaClienteId = 2;
    public const int VeiculoExistenteId = 1;
    public const int SegundoVeiculoExistenteId = 2;
    public const int ServicoExistenteId = 1000;
    public const int PecaExistenteId = 1000;
    public const string UsuarioAutenticadoId = "integration-test-user-id";
    public const string UsuarioAutenticadoNome = "integration-test-user";
    private readonly string _databaseName = $"OficinaInMemoryTests-{Guid.NewGuid()}";

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        SeedData(context);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:SecretKey"] = "TestSecretKey_Should_Be_Long_Enough_123",
                ["ConnectionStrings:DefaultConnection"] = "UseInMemory"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<OficinaDbContext>>();
            services.RemoveAll<OficinaDbContext>();
            services.AddDbContext<OficinaDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            SeedData(context);
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
                "vanessa_luna_duarte@maissaude.adm.br",
                DateTimeHelper.UTCBrazilNow()),
            PessoaFisicaClienteId);

        AddWithId(
            context,
            Cliente.Criar(
                "Betina e Fernanda Contabil Ltda",
                Documento.Parse("60.617.051/0001-99"),
                Telefone.Parse("16985344781"),
                "ouvidoria@betinaefernandacontabilltda.com.br",
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
    }

    private static void AddWithId<TEntity>(OficinaDbContext context, TEntity entity, int id)
        where TEntity : class
    {
        context.Add(entity);
        context.Entry(entity).Property("Id").CurrentValue = id;
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
