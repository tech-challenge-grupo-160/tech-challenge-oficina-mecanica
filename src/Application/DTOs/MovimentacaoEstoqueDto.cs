namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class MovimentacaoEstoqueDto
{
    public int Id { get; set; }
    public int PecaId { get; set; }
    public int? OrdemDeServicoId { get; set; }
    public int? PedidoCompraId { get; set; }
    public string NomePeca { get; set; } = null!;
    public string TipoMovimentacao { get; set; } = null!;
    public int Quantidade { get; set; }
    public int QuantidadeAnterior { get; set; }
    public int QuantidadePosterior { get; set; }
    public string Descricao { get; set; } = null!;
    public DateTime DataMovimentacao { get; set; }
}

public class MovimentacoesEstoquePorPecaDto
{
    public int PecaId { get; set; }
    public string NomePeca { get; set; } = null!;
    public string MarcaPeca { get; set; } = null!;
    public string ModeloPeca { get; set; } = null!;
    public int QuantidadeNaOrdem { get; set; }
    public int TotalMovimentacoes { get; set; }
    public List<MovimentacaoEstoqueDto> Movimentacoes { get; set; } = new();
}
