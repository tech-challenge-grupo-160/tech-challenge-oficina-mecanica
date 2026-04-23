namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class PedidoCompraDto
{
    public int Id { get; set; }
    public int OrdemDeServicoId { get; set; }
    public int PecaId { get; set; }
    public string NomePeca { get; set; } = null!;
    public int QuantidadeSolicitada { get; set; }
    public int QuantidadeRecebida { get; set; }
    public string Status { get; set; } = null!;
    public DateTime DataSolicitacao { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public string Observacao { get; set; } = null!;
}

public class CriarPedidoCompraDto
{
    public int OrdemDeServicoId { get; set; }
    public int PecaId { get; set; }
    public int QuantidadeSolicitada { get; set; }
    public string? Observacao { get; set; }
}

public class ReceberPedidoCompraDto
{
    public int QuantidadeRecebida { get; set; }
}
