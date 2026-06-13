using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterMonitoramentoOrdemDeServicoQueryHandler : IRequestHandler<ObterMonitoramentoOrdemDeServicoQuery, MonitoramentoOrdemDeServicoResult>
{
    private const string LoggerName = nameof(ObterMonitoramentoOrdemDeServicoQueryHandler);
    private readonly IClock _clock;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public ObterMonitoramentoOrdemDeServicoQueryHandler(
        IClock clock,
        IOrdemDeServicoRepository ordemRepository,
        ILoggerFactory loggerFactory)
    {
        _clock = clock;
        _ordemRepository = ordemRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<MonitoramentoOrdemDeServicoResult> Handle(ObterMonitoramentoOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterMonitoramentoAsync(query.Id, cancellationToken);
    }

    private async Task<MonitoramentoOrdemDeServicoResult> ObterMonitoramentoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return OrdemDeServicoMapper.ToMonitoramentoResult(ordem, _clock.Now);
    }
}


