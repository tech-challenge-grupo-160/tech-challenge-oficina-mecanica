using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Mappers;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.PedidosCompra;

public sealed class ListarPedidosCompraPorOrdemQueryHandler : IRequestHandler<ListarPedidosCompraPorOrdemQuery, IEnumerable<PedidoCompraDto>>
{
    private const string LoggerName = nameof(ListarPedidosCompraPorOrdemQueryHandler);
    private readonly IPedidoCompraRepository _pedidoCompraRepository;
    private readonly ILogger _logger;

    public ListarPedidosCompraPorOrdemQueryHandler(
        IPedidoCompraRepository pedidoCompraRepository,
        ILoggerFactory loggerFactory)
    {
        _pedidoCompraRepository = pedidoCompraRepository;
        _logger = loggerFactory.CreateLogger(LoggerName);
    }

    public async Task<IEnumerable<PedidoCompraDto>> Handle(ListarPedidosCompraPorOrdemQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation(LogTemplate.Start, LoggerName);
        try
        {
            _logger.LogDebug(LogTemplate.Trace, LoggerName, nameof(Handle), "Consultando pedidos de compra por ordem de servico");
            var pedidos = await _pedidoCompraRepository.ObterPorOrdemDeServicoAsync(query.OrdemDeServicoId, cancellationToken);
            var resultado = pedidos.Select(pedido => pedido.ToDto()).ToArray();
            _logger.LogInformation(LogTemplate.End, LoggerName, $"Consulta de pedidos de compra concluida. Total de registros: {resultado.Length}");
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, LogTemplate.Error, LoggerName, nameof(Handle), LogTemplate.CurrentTraceId(), ex.Message);
            throw;
        }
    }
}
