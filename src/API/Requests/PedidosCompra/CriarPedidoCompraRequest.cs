namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.PedidosCompra;

public sealed class CriarPedidoCompraRequest
{
    public int OrdemDeServicoId { get; set; }
    public int PecaId { get; set; }
    public int QuantidadeSolicitada { get; set; }
    public string? Observacao { get; set; }
}
