namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class VeiculoDto
{
    public Guid Id { get; set; }
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public Guid ClienteId { get; set; }
}

public class CriarVeiculoDto
{
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public string CpfCnpj { get; set; } = null!;
}

public class AtualizarVeiculoDto
{
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
}
