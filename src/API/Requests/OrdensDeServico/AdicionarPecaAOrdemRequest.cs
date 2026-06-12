namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;

public sealed class AdicionarPecaAOrdemRequest
{
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
}
