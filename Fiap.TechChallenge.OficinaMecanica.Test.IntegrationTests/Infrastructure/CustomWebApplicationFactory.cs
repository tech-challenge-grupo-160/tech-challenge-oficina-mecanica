using System.Security.Claims;
using System.Text.Encodings.Web;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
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

        context.Clientes.AddRange(
            new Cliente
            {
                Id = PessoaFisicaClienteId,
                Nome = "Vanessa Luna Duarte",
                CpfCnpj = DocumentoHelper.NormalizarCpf("476.548.668-01"),
                Telefone = TelefoneHelper.Normalizar("15984608796"),
                Email = "vanessa_luna_duarte@maissaude.adm.br",
                DataCadastro = DateTimeHelper.UTCBrazilNow()
            },
            new Cliente
            {
                Id = PessoaJuridicaClienteId,
                Nome = "Betina e Fernanda Contabil Ltda",
                CpfCnpj = DocumentoHelper.NormalizarCnpj("60.617.051/0001-99"),
                Telefone = TelefoneHelper.Normalizar("16985344781"),
                Email = "ouvidoria@betinaefernandacontabilltda.com.br",
                DataCadastro = DateTimeHelper.UTCBrazilNow()
            });

        context.Veiculos.AddRange(
            new Veiculo
            {
                Id = VeiculoExistenteId,
                Placa = "BRA2E19",
                Marca = "Volkswagen",
                Modelo = "Gol",
                Ano = 2020,
                ClienteId = PessoaFisicaClienteId
            },
            new Veiculo
            {
                Id = SegundoVeiculoExistenteId,
                Placa = "XYZ9A88",
                Marca = "Fiat",
                Modelo = "Argo",
                Ano = 2022,
                ClienteId = PessoaJuridicaClienteId
            });

        context.Servicos.Add(
            new Servico
            {
                Id = ServicoExistenteId,
                Nome = "Alinhamento",
                Descricao = "Servico de alinhamento",
                Preco = 150m,
                TempoEstimado = 30
            });

        context.Pecas.Add(
            new Peca
            {
                Id = PecaExistenteId,
                Nome = "Pastilha de Freio",
                Marca = "Cobreq",
                Modelo = "N-1234",
                Preco = 45m,
                QuantidadeEstoque = 100
            });

        context.SaveChanges();
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
