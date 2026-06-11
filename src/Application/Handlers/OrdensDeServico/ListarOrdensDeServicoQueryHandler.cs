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

public sealed class ListarOrdensDeServicoQueryHandler : IRequestHandler<ListarOrdensDeServicoQuery, PagedResultDto<OrdemDeServicoDto>>
{
    private const string LoggerName = nameof(ListarOrdensDeServicoQueryHandler);
    private readonly OrdemDeServicoHandlerDependencies _dependencies;
    private readonly ILogger _logger;

    public ListarOrdensDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
    {
        _dependencies = dependencies;
        _logger = dependencies.LoggerFactory.CreateLogger(LoggerName);
    }

    public Task<PagedResultDto<OrdemDeServicoDto>> Handle(ListarOrdensDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ListarOrdensDeServicoAsync(query.Page, query.PageSize, query.ClienteId, query.Status, query.Numero, query.DataAberturaInicio, query.DataAberturaFim, cancellationToken);
    }

private async Task<PagedResultDto<OrdemDeServicoDto>> ListarOrdensDeServicoAsync(
        int page,
        int pageSize,
        int? clienteId,
        string? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken)
    {
        StatusOrdemDeServico? statusFiltro = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusOrdemDeServico>(status, true, out var statusEnum))
            {
                throw new ServiceValidationException($"Status invalido: {status}");
            }

            statusFiltro = statusEnum;
        }

        var numeroFiltro = string.IsNullOrWhiteSpace(numero) ? null : numero.Trim();
        var totalItems = await _dependencies.OrdemRepository.ContarAsync(
            clienteId,
            statusFiltro,
            numeroFiltro,
            dataAberturaInicio,
            dataAberturaFim,
            cancellationToken);

        var ordens = await _dependencies.OrdemRepository.ObterPaginadoAsync(
            page,
            pageSize,
            clienteId,
            statusFiltro,
            numeroFiltro,
            dataAberturaInicio,
            dataAberturaFim,
            cancellationToken);

        return new PagedResultDto<OrdemDeServicoDto>
        {
            Items = ordens.Select(OrdemDeServicoMapper.ToDto).ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }
}
