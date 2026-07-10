using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class CriarOrdemDeServicoCommand : IRequest<OrdemDeServicoResult>
{
    public int ClienteId { get; init; }
    public int VeiculoId { get; init; }
    public string DescricaoSolicitacao { get; init; } = null!;
    public string? ObservacoesRecepcao { get; init; }
    public IReadOnlyCollection<CriarOrdemDeServicoServicoCommand> Servicos { get; init; } = [];
    public IReadOnlyCollection<CriarOrdemDeServicoPecaCommand> Pecas { get; init; } = [];
}

public sealed class CriarOrdemDeServicoServicoCommand
{
    public int ServicoId { get; init; }
}

public sealed class CriarOrdemDeServicoPecaCommand
{
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
}
