using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class PedidoCompra
{
    private PedidoCompra()
    {
    }

    public int Id { get; private set; }
    public int OrdemDeServicoId { get; private set; }
    public int PecaId { get; private set; }
    public int QuantidadeSolicitada { get; private set; }
    public int QuantidadeRecebida { get; private set; }
    public StatusPedidoCompra Status { get; private set; }
    public DateTime DataSolicitacao { get; private set; }
    public DateTime? DataRecebimento { get; private set; }
    public string Observacao { get; private set; } = null!;

    public OrdemDeServico? OrdemDeServico { get; private set; }
    public Peca? Peca { get; private set; }
    public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; private set; } = new List<MovimentacaoEstoque>();

    public static PedidoCompra Criar(
        int ordemDeServicoId,
        int pecaId,
        int quantidadeSolicitada,
        DateTime dataSolicitacao,
        string observacao)
    {
        if (ordemDeServicoId <= 0)
        {
            throw new ArgumentException("Ordem de servico do pedido de compra e obrigatoria.");
        }

        if (pecaId <= 0)
        {
            throw new ArgumentException("Peca do pedido de compra e obrigatoria.");
        }

        if (quantidadeSolicitada <= 0)
        {
            throw new ArgumentException("A quantidade solicitada deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(observacao))
        {
            throw new ArgumentException("Observacao do pedido de compra e obrigatoria.");
        }

        return new PedidoCompra
        {
            OrdemDeServicoId = ordemDeServicoId,
            PecaId = pecaId,
            QuantidadeSolicitada = quantidadeSolicitada,
            QuantidadeRecebida = 0,
            Status = StatusPedidoCompra.Pendente,
            DataSolicitacao = dataSolicitacao,
            Observacao = observacao.Trim()
        };
    }

    public void VincularPeca(Peca peca)
    {
        Peca = peca ?? throw new ArgumentNullException(nameof(peca));
    }

    public void AtualizarQuantidadeSolicitada(int quantidadeSolicitada)
    {
        if (Status != StatusPedidoCompra.Pendente)
        {
            throw new InvalidOperationException("Somente pedidos pendentes podem ter a quantidade ajustada.");
        }

        if (quantidadeSolicitada <= 0)
        {
            throw new InvalidOperationException("A quantidade solicitada deve ser maior que zero.");
        }

        QuantidadeSolicitada = quantidadeSolicitada;
    }

    public void RegistrarRecebimento(int quantidadeRecebida, DateTime dataRecebimento)
    {
        if (Status != StatusPedidoCompra.Pendente)
        {
            throw new InvalidOperationException("Somente pedidos pendentes podem receber estoque.");
        }

        if (quantidadeRecebida <= 0)
        {
            throw new InvalidOperationException("A quantidade recebida deve ser maior que zero.");
        }

        QuantidadeRecebida += quantidadeRecebida;

        if (QuantidadeRecebida >= QuantidadeSolicitada)
        {
            Status = StatusPedidoCompra.Recebido;
            DataRecebimento = dataRecebimento;
        }
    }
}
