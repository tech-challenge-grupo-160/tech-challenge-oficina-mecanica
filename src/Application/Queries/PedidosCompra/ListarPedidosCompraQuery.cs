using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;

public sealed class ListarPedidosCompraQuery : IRequest<PagedResultDto<PedidoCompraDto>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}
