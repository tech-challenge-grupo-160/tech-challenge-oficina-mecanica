namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;

public sealed class CancelarOrdemDeServicoRequest
{
    public string MotivoCancelamento { get; init; } = null!;
}
