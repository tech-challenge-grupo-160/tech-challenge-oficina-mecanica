namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;

public sealed class MovimentacaoEstoqueResponse
{
    public int Id { get; init; }
    public int PecaId { get; init; }
    public int? OrdemDeServicoId { get; init; }
    public int? PedidoCompraId { get; init; }
    public string NomePeca { get; init; } = null!;
    public string TipoMovimentacao { get; init; } = null!;
    public int Quantidade { get; init; }
    public int QuantidadeAnterior { get; init; }
    public int QuantidadePosterior { get; init; }
    public string Descricao { get; init; } = null!;
    public DateTime DataMovimentacao { get; init; }
}
