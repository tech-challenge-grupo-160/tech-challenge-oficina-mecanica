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
