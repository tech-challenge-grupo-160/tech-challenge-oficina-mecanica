using Fiap.TechChallenge.OficinaMecanica.API.Requests.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.API.Responses;
using Fiap.TechChallenge.OficinaMecanica.API.Responses.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Commands.PedidosCompra;
using Fiap.TechChallenge.OficinaMecanica.Application.Results;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;

namespace Fiap.TechChallenge.OficinaMecanica.API.Mappers;

public static class PedidoCompraApiMapper
{
    public static CriarPedidoCompraCommand ToCommand(this CriarPedidoCompraRequest request)
    {
        return new CriarPedidoCompraCommand
        {
            OrdemDeServicoId = request.OrdemDeServicoId,
            PecaId = request.PecaId,
            QuantidadeSolicitada = request.QuantidadeSolicitada,
            Observacao = request.Observacao
        };
    }

    public static ReceberPedidoCompraCommand ToCommand(this ReceberPedidoCompraRequest request, int pedidoCompraId)
    {
        return new ReceberPedidoCompraCommand
        {
            PedidoCompraId = pedidoCompraId,
            QuantidadeRecebida = request.QuantidadeRecebida
        };
    }

    public static PedidoCompraResponse ToResponse(this PedidoCompraResult result)
    {
        return new PedidoCompraResponse
        {
            Id = result.Id,
            OrdemDeServicoId = result.OrdemDeServicoId,
            PecaId = result.PecaId,
            NomePeca = result.NomePeca,
            MarcaPeca = result.MarcaPeca,
            ModeloPeca = result.ModeloPeca,
            QuantidadeSolicitada = result.QuantidadeSolicitada,
            QuantidadeRecebida = result.QuantidadeRecebida,
            Status = result.Status,
            DataSolicitacao = result.DataSolicitacao,
            DataRecebimento = result.DataRecebimento,
            Observacao = result.Observacao
        };
    }

    public static IReadOnlyCollection<PedidoCompraResponse> ToResponse(this IEnumerable<PedidoCompraResult> results)
    {
        return results.Select(result => result.ToResponse()).ToArray();
    }

    public static PagedResponse<PedidoCompraResponse> ToResponse(this PagedResult<PedidoCompraResult> result)
    {
        return new PagedResponse<PedidoCompraResponse>
        {
            Items = result.Items.Select(pedido => pedido.ToResponse()).ToArray(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }
}
