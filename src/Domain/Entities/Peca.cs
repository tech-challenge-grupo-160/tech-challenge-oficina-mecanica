namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Peca
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }

    // Navigation
    public ICollection<OrdemDeServicoPeca> OrdensDeServico { get; set; } = new List<OrdemDeServicoPeca>();
}
