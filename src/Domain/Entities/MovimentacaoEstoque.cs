using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class MovimentacaoEstoque
{
    private MovimentacaoEstoque()
    {
    }

    public int Id { get; private set; }
    public int PecaId { get; private set; }
    public int? OrdemDeServicoId { get; private set; }
    public int? PedidoCompraId { get; private set; }
    public TipoMovimentacaoEstoque TipoMovimentacao { get; private set; }
    public int Quantidade { get; private set; }
    public int QuantidadeAnterior { get; private set; }
    public int QuantidadePosterior { get; private set; }
    public string Descricao { get; private set; } = null!;
    public DateTime DataMovimentacao { get; private set; }

    public Peca? Peca { get; private set; }
    public OrdemDeServico? OrdemDeServico { get; private set; }
    public PedidoCompra? PedidoCompra { get; private set; }

    public static MovimentacaoEstoque Registrar(
        int pecaId,
        int? ordemDeServicoId,
        int? pedidoCompraId,
        TipoMovimentacaoEstoque tipoMovimentacao,
        int quantidade,
        int quantidadeAnterior,
        int quantidadePosterior,
        string descricao,
        DateTime dataMovimentacao)
    {
        if (pecaId <= 0)
        {
            throw new ArgumentException("Peca da movimentacao e obrigatoria.");
        }

        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade da movimentacao deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("Descricao da movimentacao e obrigatoria.");
        }

        return new MovimentacaoEstoque
        {
            PecaId = pecaId,
            OrdemDeServicoId = ordemDeServicoId,
            PedidoCompraId = pedidoCompraId,
            TipoMovimentacao = tipoMovimentacao,
            Quantidade = quantidade,
            QuantidadeAnterior = quantidadeAnterior,
            QuantidadePosterior = quantidadePosterior,
            Descricao = descricao.Trim(),
            DataMovimentacao = dataMovimentacao
        };
    }
}
