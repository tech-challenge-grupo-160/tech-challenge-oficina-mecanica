using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Security;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Catalogo;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Security;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Time;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Observability;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;
using Amazon.SQS;
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
        var execucaoBaseUrl = configuration["Services:Execucao:BaseUrl"]
            ?? "http://localhost:8082";
        services
            .AddHttpClient<ICatalogoExecucao, CatalogoExecucaoHttpClient>(client =>
            {
                client.BaseAddress = new Uri(execucaoBaseUrl, UriKind.Absolute);
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
                options.Retry.MaxRetryAttempts = 2;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
            });
        var messagingOptions = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ?? new MessagingOptions();
        services.Configure<MessagingOptions>(
            configuration.GetSection(MessagingOptions.SectionName));
        services.AddSingleton<IAmazonSQS>(_ => AwsMessagingClientFactory.CreateSqs(messagingOptions));
        services.AddScoped<IInicioDiagnosticoScheduler, InicioDiagnosticoScheduler>();
        services.AddHostedService<OutboxSqsPublisherWorker>();
        services.AddHostedService<DiagnosticoIniciadoConsumerWorker>();
        services.AddHostedService<DiagnosticoConcluidoConsumerWorker>();
        services.AddHostedService<OrcamentoGeradoConsumerWorker>();
        services.AddHostedService<OrcamentoAprovadoConsumerWorker>();
        services.AddHostedService<OrcamentoRecusadoConsumerWorker>();
        services.AddHostedService<PecasReservadasConsumerWorker>();
        services.AddHostedService<EstoqueInsuficienteConsumerWorker>();
        services.AddHostedService<ExecucaoFinalizadaConsumerWorker>();
        services.AddHostedService<PagamentoConfirmadoConsumerWorker>();
        services.AddScoped<IOrdemDeServicoRepository, OrdemDeServicoRepository>();
        services.AddScoped<IOrdemServicoHistoricoRepository, OrdemServicoHistoricoRepository>();
        services.AddScoped<INotificacaoClienteRepository, NotificacaoClienteRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITransactionManager, EfTransactionManager>();
        services.AddSingleton<IClock, BrazilClock>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IBusinessMetrics, DogStatsDBusinessMetrics>();

        return services;
    }
}
