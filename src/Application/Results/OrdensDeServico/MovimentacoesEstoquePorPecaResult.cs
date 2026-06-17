namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

public sealed class MovimentacoesEstoquePorPecaResult
{
    public int PecaId { get; init; }
    public string NomePeca { get; init; } = null!;
    public string MarcaPeca { get; init; } = null!;
    public string ModeloPeca { get; init; } = null!;
    public int QuantidadeNaOrdem { get; init; }
    public int TotalMovimentacoes { get; init; }
    public List<MovimentacaoEstoqueResult> Movimentacoes { get; init; } = new();
}
