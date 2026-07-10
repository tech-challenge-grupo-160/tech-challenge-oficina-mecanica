namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;

public sealed class MovimentacoesEstoquePorPecaResponse
{
    public int PecaId { get; init; }
    public string NomePeca { get; init; } = null!;
    public string MarcaPeca { get; init; } = null!;
    public string ModeloPeca { get; init; } = null!;
    public int QuantidadeNaOrdem { get; init; }
    public int TotalMovimentacoes { get; init; }
    public List<MovimentacaoEstoqueResponse> Movimentacoes { get; init; } = new();
}
