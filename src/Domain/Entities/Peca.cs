namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Peca
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }

    public void BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade para baixa deve ser maior que zero.");
        }

        if (QuantidadeEstoque < quantidade)
        {
            throw new InvalidOperationException("Quantidade insuficiente em estoque.");
        }

        QuantidadeEstoque -= quantidade;
    }

    public void ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new InvalidOperationException("A quantidade para reposicao deve ser maior que zero.");
        }

        QuantidadeEstoque += quantidade;
    }

    public ICollection<OrdemDeServicoPeca> OrdensDeServico { get; set; } = new List<OrdemDeServicoPeca>();
    public ICollection<PedidoCompra> PedidosCompra { get; set; } = new List<PedidoCompra>();
    public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = new List<MovimentacaoEstoque>();
}
