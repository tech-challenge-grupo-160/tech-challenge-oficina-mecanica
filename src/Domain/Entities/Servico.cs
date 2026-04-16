namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Servico
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }

    // Navigation
    public ICollection<OrdemDeServicoServico> OrdensDeServico { get; set; } = new List<OrdemDeServicoServico>();
}
