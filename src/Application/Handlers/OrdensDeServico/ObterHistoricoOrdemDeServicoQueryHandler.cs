using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
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

public sealed class ObterHistoricoOrdemDeServicoQueryHandler : IRequestHandler<ObterHistoricoOrdemDeServicoQuery, IEnumerable<OrdemServicoHistoricoDto>>
{
    private const string LoggerName = nameof(ObterHistoricoOrdemDeServicoQueryHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public ObterHistoricoOrdemDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<IEnumerable<OrdemServicoHistoricoDto>> Handle(ObterHistoricoOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterHistoricoAsync(query.Id, cancellationToken);
    }

private async Task<IEnumerable<OrdemServicoHistoricoDto>> ObterHistoricoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _dependencies.OrdemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        var historicos = await _dependencies.HistoricoRepository.ObterPorOrdemDeServicoAsync(id, cancellationToken);
        return historicos.Select(OrdemDeServicoMapper.ToDto);
    }
}
