namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;

public sealed class CriarOrdemDeServicoRequest
{
    public int ClienteId { get; init; }
    public int VeiculoId { get; init; }
    public string DescricaoSolicitacao { get; init; } = null!;
    public string? ObservacoesRecepcao { get; init; }
}
