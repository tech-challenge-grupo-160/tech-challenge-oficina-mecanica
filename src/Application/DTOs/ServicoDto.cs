namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class ServicoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}

public class CriarServicoDto
{
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}

public class AtualizarServicoDto
{
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public int TempoEstimado { get; set; }
}
