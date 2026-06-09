namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Veiculos;

public class CriarVeiculoRequest
{
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public string CpfCnpj { get; set; } = null!;
}
