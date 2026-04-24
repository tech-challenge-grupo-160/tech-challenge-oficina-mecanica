using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class PedidoCompra
{
    public int Id { get; set; }
    public int OrdemDeServicoId { get; set; }
    public int PecaId { get; set; }
    public int QuantidadeSolicitada { get; set; }
    public int QuantidadeRecebida { get; set; }
    public StatusPedidoCompra Status { get; set; }
    public DateTime DataSolicitacao { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public string Observacao { get; set; } = null!;

    public OrdemDeServico? OrdemDeServico { get; set; }
    public Peca? Peca { get; set; }
    public ICollection<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = new List<MovimentacaoEstoque>();

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

    public void RegistrarRecebimento(int quantidadeRecebida)
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
            DataRecebimento = DateTimeHelper.UTCBrazilNow();
        }
    }
}
