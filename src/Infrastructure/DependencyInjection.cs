using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Security;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=postgres;Database=oficina_mecanica;Username=postgres;Password=postgres";
        var usingInMemoryDatabase = environment.IsEnvironment("Testing") ||
            string.Equals(connectionString, "UseInMemory", StringComparison.OrdinalIgnoreCase);

        if (usingInMemoryDatabase)
        {
            services.AddDbContext<OficinaDbContext>(options =>
                options.UseInMemoryDatabase("OficinaInMemory"));
        }
        else
        {
            services.AddDbContext<OficinaDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services
            .AddHealthChecks()
            .AddDbContextCheck<OficinaDbContext>(
                name: "Database",
                failureStatus: HealthStatus.Unhealthy);

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();
        services.AddScoped<IPecaRepository, PecaRepository>();
        services.AddScoped<IOrdemDeServicoRepository, OrdemDeServicoRepository>();
        services.AddScoped<IOrdemServicoHistoricoRepository, OrdemServicoHistoricoRepository>();
        services.AddScoped<INotificacaoClienteRepository, NotificacaoClienteRepository>();
        services.AddScoped<IPedidoCompraRepository, PedidoCompraRepository>();
        services.AddScoped<IMovimentacaoEstoqueRepository, MovimentacaoEstoqueRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITransactionManager, EfTransactionManager>();
        services.AddSingleton<IClock, BrazilClock>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
