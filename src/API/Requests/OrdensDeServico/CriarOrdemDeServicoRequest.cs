namespace Fiap.TechChallenge.OficinaMecanica.API.Requests.OrdensDeServico;

public sealed class CriarOrdemDeServicoRequest
{
    public int ClienteId { get; init; }
    public int VeiculoId { get; init; }
    public string DescricaoSolicitacao { get; init; } = null!;
    public string? ObservacoesRecepcao { get; init; }
    public IReadOnlyCollection<CriarOrdemDeServicoServicoRequest> Servicos { get; init; } = [];
    public IReadOnlyCollection<CriarOrdemDeServicoPecaRequest> Pecas { get; init; } = [];
}

public sealed class CriarOrdemDeServicoServicoRequest
{
    public int ServicoId { get; init; }
}

public sealed class CriarOrdemDeServicoPecaRequest
{
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
}
