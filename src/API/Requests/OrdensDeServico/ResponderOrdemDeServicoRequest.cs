namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;

public sealed class ResponderOrdemDeServicoRequest
{
    public bool Aprovado { get; init; }
    public string? MotivoRecusa { get; init; }
}
