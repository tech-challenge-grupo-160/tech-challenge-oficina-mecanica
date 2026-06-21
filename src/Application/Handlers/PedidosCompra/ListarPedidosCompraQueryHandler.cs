using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Results;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.PedidosCompra;

public sealed class ListarPedidosCompraQueryHandler : IRequestHandler<ListarPedidosCompraQuery, PagedResult<PedidoCompraResult>>
{
    private const string LoggerName = nameof(ListarPedidosCompraQueryHandler);
    private readonly IPedidoCompraRepository _pedidoCompraRepository;
    private readonly ILogger _logger;

    public ListarPedidosCompraQueryHandler(
        IPedidoCompraRepository pedidoCompraRepository,
        ILoggerFactory loggerFactory)
    {
        _pedidoCompraRepository = pedidoCompraRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<PagedResult<PedidoCompraResult>> Handle(ListarPedidosCompraQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando pedidos de compra de forma paginada");
            var totalItems = await _pedidoCompraRepository.ContarAsync(cancellationToken);
            var pedidos = await _pedidoCompraRepository.ObterPaginadoAsync(query.Page, query.PageSize, cancellationToken);

            var resultado = new PagedResult<PedidoCompraResult>
            {
                Items = pedidos.Select(pedido => pedido.ToResult()).ToArray(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize)
            };

            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta paginada de pedidos de compra concluida. Total de registros: {totalItems}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
