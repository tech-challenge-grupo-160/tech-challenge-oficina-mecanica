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

public sealed class ObterOrdemDeServicoPorIdQueryHandler : IRequestHandler<ObterOrdemDeServicoPorIdQuery, OrdemDeServicoDto>
{
    private const string LoggerName = nameof(ObterOrdemDeServicoPorIdQueryHandler);
    private readonly IOrdemDeServicoRepository _ordemRepository;
    private readonly ILogger _logger;

    public ObterOrdemDeServicoPorIdQueryHandler(
        IOrdemDeServicoRepository ordemRepository,
        ILoggerFactory loggerFactory)
    {
        _ordemRepository = ordemRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public Task<OrdemDeServicoDto> Handle(ObterOrdemDeServicoPorIdQuery query, CancellationToken cancellationToken)
    {
        return ObterOrdemDeServicoAsync(query.Id, cancellationToken);
    }

    private async Task<OrdemDeServicoDto> ObterOrdemDeServicoAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _ordemRepository.ObterPorIdAsync(id, cancellationToken);
        if (ordem == null)
        {
            throw new ServiceNotFoundException($"Ordem de servico com ID {id} nao encontrada.");
        }

        return OrdemDeServicoMapper.ToDto(ordem);
    }
}


