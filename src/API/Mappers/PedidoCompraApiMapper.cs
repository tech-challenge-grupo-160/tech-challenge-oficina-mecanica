using Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class PedidoCompraApiMapper
{
    public static CriarPedidoCompraCommand ToCommand(this CriarPedidoCompraDto dto)
    {
        return new CriarPedidoCompraCommand
        {
            OrdemDeServicoId = dto.OrdemDeServicoId,
            PecaId = dto.PecaId,
            QuantidadeSolicitada = dto.QuantidadeSolicitada,
            Observacao = dto.Observacao
        };
    }

    public static ReceberPedidoCompraCommand ToCommand(this ReceberPedidoCompraDto dto, int pedidoCompraId)
    {
        return new ReceberPedidoCompraCommand
        {
            PedidoCompraId = pedidoCompraId,
            QuantidadeRecebida = dto.QuantidadeRecebida
        };
    }
}
