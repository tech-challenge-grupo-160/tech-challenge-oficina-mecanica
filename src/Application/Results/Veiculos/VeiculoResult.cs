namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;

public class VeiculoResult
{
    public int Id { get; set; }
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public int ClienteId { get; set; }
}
