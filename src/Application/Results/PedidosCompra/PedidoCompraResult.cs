namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.PedidosCompra;

public sealed class PedidoCompraResult
{
    public int Id { get; init; }
    public int OrdemDeServicoId { get; init; }
    public int PecaId { get; init; }
    public string NomePeca { get; init; } = null!;
    public string MarcaPeca { get; init; } = null!;
    public string ModeloPeca { get; init; } = null!;
    public int QuantidadeSolicitada { get; init; }
    public int QuantidadeRecebida { get; init; }
    public string Status { get; init; } = null!;
    public DateTime DataSolicitacao { get; init; }
    public DateTime? DataRecebimento { get; init; }
    public string Observacao { get; init; } = null!;
}
