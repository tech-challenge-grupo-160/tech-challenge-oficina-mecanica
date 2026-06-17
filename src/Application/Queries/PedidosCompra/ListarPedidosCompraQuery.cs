using Fiap.TechChallenge.OficinaMecanica.Application.Results;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.PedidosCompra;

public sealed class ListarPedidosCompraQuery : IRequest<PagedResult<PedidoCompraResult>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}
