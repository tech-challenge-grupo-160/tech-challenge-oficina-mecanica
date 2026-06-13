using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Servicos;

public sealed class ObterServicoPorIdQueryHandler : IRequestHandler<ObterServicoPorIdQuery, ServicoResult>
{
    private const string LoggerName = nameof(ObterServicoPorIdQueryHandler);
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public ObterServicoPorIdQueryHandler(IServicoRepository servicoRepository, ILoggerFactory loggerFactory)
    {
        _servicoRepository = servicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<ServicoResult> Handle(ObterServicoPorIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando servico por identificador");
            var servico = await _servicoRepository.ObterPorIdAsync(query.Id, cancellationToken);
            if (servico == null)
            {
                _logger.LogWarning(LogTemplate.Warning, LoggerName, nameof(Handle), "Servico nao encontrado para o identificador informado");
                throw new ServiceNotFoundException($"Servico com ID {query.Id} nao encontrado.");
            }

            _logger.LogInformation(LogTemplate.End, LoggerName, "Servico obtido com sucesso.");
            return servico.ToResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
