namespace Fiap.TechChallenge.OficinaMecanica.API.Responses.OrdensDeServico;

public sealed class OrdemDeServicoPecaResponse
{
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
    public decimal Preco { get; init; }
}
