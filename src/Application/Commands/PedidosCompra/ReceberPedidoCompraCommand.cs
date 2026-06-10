using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;

public sealed class ReceberPedidoCompraCommand : IRequest<PedidoCompraDto>
{
    public int PedidoCompraId { get; init; }
    public int QuantidadeRecebida { get; init; }
}
