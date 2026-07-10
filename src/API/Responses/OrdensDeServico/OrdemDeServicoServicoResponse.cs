namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;

public sealed class OrdemDeServicoServicoResponse
{
    public int ServicoId { get; init; }
    public decimal Preco { get; init; }
    public int TempoEstimado { get; init; }
}
