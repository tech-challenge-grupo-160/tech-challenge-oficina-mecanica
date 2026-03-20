namespace oficina_mecanica.Domain.Entities;

public class Peca
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }

    // Navigation
    public ICollection<OrdemDeServicoPeca> OrdensDeServico { get; set; } = new List<OrdemDeServicoPeca>();
}
