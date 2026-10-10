using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Microsoft.Extensions.Logging;
using StatsdClient;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Observability;

public sealed class DogStatsDBusinessMetrics : IBusinessMetrics, IDisposable
{
    private const string OrdersCreatedMetric = "oficina_mecanica.orders.created";
    private const string OrderFailuresMetric = "oficina_mecanica.orders.creation_failed";
    private const string OrderStageDurationMetric = "oficina_mecanica.orders.stage_duration";
    private readonly DogStatsdService _client;

    public DogStatsDBusinessMetrics(ILogger<DogStatsDBusinessMetrics> logger)
    {
        _client = new DogStatsdService();
        // DD_SERVICE e DD_ENV (definidos no Dockerfile/Deployment) ja sao lidos
        // pelo client para o unified service tagging; declarar ConstantTags
        // aqui duplicaria as tags "service"/"env" com valores potencialmente
        // divergentes (ex.: ASPNETCORE_ENVIRONMENT em vez do DD_ENV do ambiente).
        var configured = _client.Configure(
            new StatsdConfig
            {
                StatsdServerName = Environment.GetEnvironmentVariable("DD_AGENT_HOST") ?? "127.0.0.1",
                StatsdPort = GetPort(),
                Prefix = string.Empty
            },
            exception => logger.LogWarning(exception, "Falha ao configurar cliente DogStatsD."));

        if (!configured)
        {
            logger.LogWarning("DogStatsD nao foi configurado; metricas de negocio nao serao enviadas.");
        }
    }

    public void RecordOrderCreated()
    {
        _client.Increment(OrdersCreatedMetric);
    }

    public void RecordOrderCreationFailure(string reason)
    {
        _client.Increment(OrderFailuresMetric, tags: [$"reason:{reason}"]);
    }

    public void RecordOrderStageDuration(string stage, double durationSeconds)
    {
        if (durationSeconds < 0)
        {
            return;
        }

        _client.Distribution(
            OrderStageDurationMetric,
            durationSeconds,
            tags: [$"stage:{stage}"]);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private static int GetPort()
    {
        return int.TryParse(Environment.GetEnvironmentVariable("DD_DOGSTATSD_PORT"), out var port)
            ? port
            : 8125;
    }
}
