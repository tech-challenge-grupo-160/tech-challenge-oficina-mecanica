using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;

public sealed class ListarPedidosCompraPorOrdemQuery : IRequest<IEnumerable<PedidoCompraDto>>
{
    public int OrdemDeServicoId { get; init; }
}
