using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Exceptions;
using Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Services.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterHistoricoOrdemDeServicoQueryHandler : IRequestHandler<ObterHistoricoOrdemDeServicoQuery, IEnumerable<OrdemServicoHistoricoResult>>
{
    private const string LoggerName = nameof(ObterHistoricoOrdemDeServicoQueryHandler);
    private readonly IOrdemServicoHistoricoRepository _historicoRepository;
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public ObterHistoricoOrdemDeServicoQueryHandler(
        IOrdemServicoHistoricoRepository historicoRepository,
        IOrdemDeServicoRepository ordemRepository,
        ILoggerFactory loggerFactory)
    {
        _historicoRepository = historicoRepository;
        _ordemRepository = ordemRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<IEnumerable<OrdemServicoHistoricoResult>> Handle(ObterHistoricoOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterHistoricoAsync(query.Id, cancellationToken);
    }

    private async Task<IEnumerable<OrdemServicoHistoricoResult>> ObterHistoricoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var historicos = await _historicoRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return historicos.Select(OrdemDeServicoMapper.ToResult);
    }
}


