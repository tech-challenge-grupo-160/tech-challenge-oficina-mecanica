using Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;

public sealed class ReceberPedidoCompraCommand : IRequest<PedidoCompraResult>
{
    public int PedidoCompraId { get; init; }
    public int QuantidadeRecebida { get; init; }
}
