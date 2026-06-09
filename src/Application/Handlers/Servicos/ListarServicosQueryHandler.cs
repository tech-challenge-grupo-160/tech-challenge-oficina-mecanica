using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Servicos;

public sealed class ListarServicosQueryHandler : IRequestHandler<ListarServicosQuery, IEnumerable<ServicoResult>>
{
    private const string LoggerName = nameof(ListarServicosQueryHandler);
    private readonly IServicoRepository _servicoRepository;
    private readonly ILogger _logger;

    public ListarServicosQueryHandler(IServicoRepository servicoRepository, ILoggerFactory loggerFactory)
    {
        _servicoRepository = servicoRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<IEnumerable<ServicoResult>> Handle(ListarServicosQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando todos os servicos");
            var servicos = await _servicoRepository.ObterTodosAsync(cancellationToken);
            var resultado = servicos.Select(servico => servico.ToResult()).ToArray();
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta de servicos concluida. Total de registros: {resultado.Length}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
