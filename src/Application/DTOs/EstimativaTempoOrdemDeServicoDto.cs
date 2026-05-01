namespace Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

public class EstimativaTempoOrdemDeServicoDto
{
    public int OrdemDeServicoId { get; set; }
    public string Numero { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int TotalServicos { get; set; }
    public int TempoEstimadoMinutos { get; set; }
    public double TempoEstimadoHoras { get; set; }
    public List<EstimativaTempoServicoDto> Servicos { get; set; } = new();
}

public class EstimativaTempoServicoDto
{
    public int ServicoId { get; set; }
    public int TempoEstimadoMinutos { get; set; }
    public double TempoEstimadoHoras { get; set; }
}
