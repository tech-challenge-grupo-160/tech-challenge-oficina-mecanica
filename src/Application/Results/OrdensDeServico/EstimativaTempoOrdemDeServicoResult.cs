namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

public sealed class EstimativaTempoOrdemDeServicoResult
{
    public int OrdemDeServicoId { get; init; }
    public string Numero { get; init; } = null!;
    public string Status { get; init; } = null!;
    public int TotalServicos { get; init; }
    public int TempoEstimadoMinutos { get; init; }
    public double TempoEstimadoHoras { get; init; }
    public List<EstimativaTempoServicoResult> Servicos { get; init; } = new();
}
