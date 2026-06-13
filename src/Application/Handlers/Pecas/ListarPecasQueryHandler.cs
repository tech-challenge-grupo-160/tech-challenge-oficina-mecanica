using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Pecas;

public sealed class ListarPecasQueryHandler : IRequestHandler<ListarPecasQuery, IEnumerable<PecaResult>>
{
    private const string LoggerName = nameof(ListarPecasQueryHandler);
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public ListarPecasQueryHandler(IPecaRepository pecaRepository, ILoggerFactory loggerFactory)
    {
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<IEnumerable<PecaResult>> Handle(ListarPecasQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando todas as pecas");
            var pecas = await _pecaRepository.ObterTodosAsync(cancellationToken);
            var resultado = pecas.Select(peca => peca.ToResult()).ToArray();
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta de pecas concluida. Total de registros: {resultado.Length}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
