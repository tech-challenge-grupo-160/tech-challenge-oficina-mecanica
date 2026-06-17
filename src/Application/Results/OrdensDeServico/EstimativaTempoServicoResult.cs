namespace Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;

public sealed class EstimativaTempoServicoResult
{
    public int ServicoId { get; init; }
    public int TempoEstimadoMinutos { get; init; }
    public double TempoEstimadoHoras { get; init; }
}
