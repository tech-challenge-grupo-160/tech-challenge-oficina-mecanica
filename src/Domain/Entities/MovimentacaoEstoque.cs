using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public int PecaId { get; set; }
    public int? OrdemDeServicoId { get; set; }
    public int? PedidoCompraId { get; set; }
    public TipoMovimentacaoEstoque TipoMovimentacao { get; set; }
    public int Quantidade { get; set; }
    public int QuantidadeAnterior { get; set; }
    public int QuantidadePosterior { get; set; }
    public string Descricao { get; set; } = null!;
    public DateTime DataMovimentacao { get; set; }

    public Peca? Peca { get; set; }
    public OrdemDeServico? OrdemDeServico { get; set; }
    public PedidoCompra? PedidoCompra { get; set; }
}
