using Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Mappers;

public static class PedidoCompraMapper
{
    public static PedidoCompraResult ToResult(this PedidoCompra pedidoCompra)
    {
        return new PedidoCompraResult
        {
            Id = pedidoCompra.Id,
            OrdemDeServicoId = pedidoCompra.OrdemDeServicoId,
            PecaId = pedidoCompra.PecaId,
            NomePeca = pedidoCompra.Peca?.Nome ?? string.Empty,
            MarcaPeca = pedidoCompra.Peca?.Marca ?? string.Empty,
            ModeloPeca = pedidoCompra.Peca?.Modelo ?? string.Empty,
            QuantidadeSolicitada = pedidoCompra.QuantidadeSolicitada,
            QuantidadeRecebida = pedidoCompra.QuantidadeRecebida,
            Status = pedidoCompra.Status.ToString(),
            DataSolicitacao = pedidoCompra.DataSolicitacao,
            DataRecebimento = pedidoCompra.DataRecebimento,
            Observacao = pedidoCompra.Observacao
        };
    }
}
