using System.Diagnostics;
using System.Globalization;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.API.Middleware;

/// <summary>
/// Abre o contexto de log da requisicao (numero da OS da rota e
/// <c>X-Correlation-Id</c>) antes de executar o endpoint, entao todos os logs
/// da requisicao saem com esses campos, e registra o resultado no fim.
/// </summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    private static readonly string[] RotasComOrdemServico = ["ordemServicoId"];

    public async Task InvokeAsync(HttpContext context)
    {
        using var contexto = logger.Iniciar(
            LerCorrelationId(context),
            LerOrdemServicoId(context));

        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;
        var logLevel = statusCode >= StatusCodes.Status500InternalServerError
            ? LogLevel.Error
            : statusCode >= StatusCodes.Status400BadRequest
                ? LogLevel.Warning
                : LogLevel.Information;

        logger.Log(
            logLevel,
            "HTTP request completed {method} {rota} with status {statusHttp} in {duracao} ms",
            context.Request.Method,
            context.Request.Path.Value ?? "/",
            statusCode,
            Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2));
    }

    private static Guid? LerCorrelationId(HttpContext context) =>
        Guid.TryParse(context.Request.Headers[LogContexto.CorrelationIdHeader], out var correlationId)
            ? correlationId
            : null;

    private static int? LerOrdemServicoId(HttpContext context)
    {
        // Em /api/v1/ordens-servico/{id} o id da rota e o numero da OS.
        var chaves = string.Equals(
            Convert.ToString(context.Request.RouteValues["controller"], CultureInfo.InvariantCulture),
            "OrdensDeServico",
            StringComparison.Ordinal)
            ? RotasComOrdemServico.Append("id")
            : RotasComOrdemServico;

        foreach (var chave in chaves)
        {
            if (context.Request.RouteValues.TryGetValue(chave, out var valor) &&
                int.TryParse(
                    Convert.ToString(valor, CultureInfo.InvariantCulture),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var ordemServicoId))
                return ordemServicoId;
        }

        return null;
    }
}
