namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;

public sealed class EstimativaTempoServicoResponse
{
    public int ServicoId { get; init; }
    public int TempoEstimadoMinutos { get; init; }
    public double TempoEstimadoHoras { get; init; }
}
