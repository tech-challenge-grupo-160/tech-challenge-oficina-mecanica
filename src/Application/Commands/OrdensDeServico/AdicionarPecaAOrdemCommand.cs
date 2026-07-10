using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class AdicionarPecaAOrdemCommand : IRequest<OrdemDeServicoResult>
{
    public int OrdemDeServicoId { get; init; }
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
}

