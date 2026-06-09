using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.Clientes;

public sealed class ListarClientesQueryHandler : IRequestHandler<ListarClientesQuery, PagedResultDto<ClienteResult>>
{
    private const string LoggerName = nameof(ListarClientesQueryHandler);
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger _logger;

    public ListarClientesQueryHandler(
        IClienteRepository clienteRepository,
        ILoggerFactory loggerFactory)
    {
        _clienteRepository = clienteRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PagedResultDto<ClienteResult>> Handle(ListarClientesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Normalizando filtros de busca");
            var documentoFiltro = NormalizarDocumentoParaBusca(query.CpfCnpj);
            var nomeFiltro = string.IsNullOrWhiteSpace(query.Nome) ? null : query.Nome.Trim();

            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando clientes paginados");
            var totalItems = await _clienteRepository.ContarAsync(nomeFiltro, documentoFiltro, cancellationToken);
            var clientes = await _clienteRepository.ObterPaginadoAsync(query.Page, query.PageSize, nomeFiltro, documentoFiltro, cancellationToken);

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta paginada concluida. Total de registros: {totalItems}");
            return new PagedResultDto<ClienteResult>
            {
                Items = clientes.Select(cliente => cliente.ToResult()).ToArray(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }

    private static string? NormalizarDocumentoParaBusca(string? cpfCnpj)
    {
        if (string.IsNullOrWhiteSpace(cpfCnpj))
        {
            return null;
        }

        var normalizado = new string(cpfCnpj
            .Trim()
            .Where(c => char.IsDigit(c) || char.IsLetter(c))
            .Select(char.ToUpperInvariant)
            .ToArray());

        return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
    }
}
