using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;

public sealed class CriarPedidoCompraCommand : IRequest<PedidoCompraDto>
{
    public int OrdemDeServicoId { get; init; }
    public int PecaId { get; init; }
    public int QuantidadeSolicitada { get; init; }
    public string? Observacao { get; init; }
}
