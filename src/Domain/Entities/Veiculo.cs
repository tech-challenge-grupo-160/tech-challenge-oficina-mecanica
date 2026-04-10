namespace Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

public class Veiculo
{
    public Guid Id { get; set; }
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public Guid ClienteId { get; set; }

    // Navigation
    public Cliente? Cliente { get; set; }
    public ICollection<OrdemDeServico> OrdensDeServico { get; set; } = new List<OrdemDeServico>();
}
