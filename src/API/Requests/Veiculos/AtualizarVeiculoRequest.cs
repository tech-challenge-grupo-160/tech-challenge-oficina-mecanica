namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.Veiculos;

public class AtualizarVeiculoRequest
{
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
}
