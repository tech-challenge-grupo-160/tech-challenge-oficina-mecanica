namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class CriarVeiculoParaClienteDto
{
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
}
