using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Pecas;

public sealed class ObterPecaPorIdQueryHandler : IRequestHandler<ObterPecaPorIdQuery, PecaResult>
{
    private const string LoggerName = nameof(ObterPecaPorIdQueryHandler);
    private readonly IPecaRepository _pecaRepository;
    private readonly ILogger _logger;

    public ObterPecaPorIdQueryHandler(IPecaRepository pecaRepository, ILoggerFactory loggerFactory)
    {
        _pecaRepository = pecaRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PecaResult> Handle(ObterPecaPorIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando peca por identificador");
            var peca = await _pecaRepository.ObterPorIdAsync(query.Id, cancellationToken);
            if (peca == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Peca nao encontrada para o identificador informado");
                throw new ServiceNotFoundException($"Peca com ID {query.Id} nao encontrada.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Peca obtida com sucesso.");
            return peca.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
