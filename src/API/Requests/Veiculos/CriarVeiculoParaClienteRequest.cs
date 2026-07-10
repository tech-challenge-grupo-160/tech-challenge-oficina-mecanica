namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Veiculos;

public class CriarVeiculoParaClienteRequest
{
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
}
