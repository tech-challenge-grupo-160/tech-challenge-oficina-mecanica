using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data.Seeders;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Extensions;

/// <summary>
/// Extensões para IHost para executar migrations e seeding na inicialização da aplicação.
/// </summary>
public static class HostExtensions
{
    private const string LoggerName = nameof(HostExtensions);

    /// <summary>
    /// Executa migrations pendentes e seed dos dados iniciais.
    /// </summary>
    /// <param name="host">O host da aplicação</param>
    /// <param name="isDevelopment">Se está em ambiente de desenvolvimento</param>
    /// <returns>Retorna o host para permitir encadeamento</returns>
    public static async Task<IHost> MigrateAndSeedAsync(this IHost host, bool isDevelopment = false)
    {
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(LoggerName);

        try
        {
            logger.LogInformation(LogTemplate.Start, LoggerName);
            logger.LogDebug(
                LogTemplate.Trace,
                LoggerName,
                nameof(MigrateAndSeedAsync),
                "Iniciando processo de migration e seeding do banco de dados");

            var context = serviceProvider.GetRequiredService<OficinaDbContext>();

            // Executar migrations com retry para aguardar o banco estar pronto
            await MigrateWithRetryAsync(context, logger, isDevelopment);

            // Executar seeding apenas em desenvolvimento
            if (isDevelopment)
            {
                logger.LogDebug(
                    LogTemplate.Trace,
                    LoggerName,
                    nameof(MigrateAndSeedAsync),
                    "Iniciando seeding de dados mocados");
                await OficinaDbContextSeeder.SeedAsync(context);
                logger.LogInformation(LogTemplate.End, LoggerName, "Seeding completado com sucesso.");
            }

            logger.LogInformation(LogTemplate.End, LoggerName, "Migration e seeding finalizados com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                LogTemplate.Error,
                LoggerName,
                nameof(MigrateAndSeedAsync),
                LogTemplate.CurrentTraceId(),
                "Erro critico ao executar migrations e seeding. A aplicacao sera encerrada.");
            throw;
        }

        return host;
    }

    /// <summary>
    /// Executa migrations com retry, aguardando até 30 segundos pela disponibilidade do banco.
    /// </summary>
    private static async Task MigrateWithRetryAsync(
        OficinaDbContext context,
        ILogger logger,
        bool isDevelopment,
        int maxRetries = 6,
        int delayMilliseconds = 5000)
    {
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                logger.LogDebug(
                    LogTemplate.Trace,
                    LoggerName,
                    nameof(MigrateWithRetryAsync),
                    $"Tentativa {attempt + 1}/{maxRetries} de executar migrations");

                await context.Database.MigrateAsync();

                logger.LogInformation(LogTemplate.End, LoggerName, "Migrations executadas com sucesso.");
                return;
            }
            catch (Exception ex)
            {
                attempt++;

                if (attempt >= maxRetries)
                {
                    logger.LogError(
                        ex,
                        LogTemplate.Error,
                        LoggerName,
                        nameof(MigrateWithRetryAsync),
                        LogTemplate.CurrentTraceId(),
                        $"Falha ao conectar ao banco de dados apos {maxRetries} tentativas.");
                    throw;
                }

                logger.LogWarning(
                    ex,
                    LogTemplate.Warning,
                    LoggerName,
                    nameof(MigrateWithRetryAsync),
                    $"Falha ao conectar ao banco de dados. Tentativa {attempt}/{maxRetries}. Aguardando {delayMilliseconds / 1000}s antes de tentar novamente.");

                await Task.Delay(delayMilliseconds);
            }
        }
    }
}
