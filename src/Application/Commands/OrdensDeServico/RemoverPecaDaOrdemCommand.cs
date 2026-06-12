using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class RemoverPecaDaOrdemCommand : IRequest<OrdemDeServicoResult>
{
    public int OrdemDeServicoId { get; init; }
    public int PecaId { get; init; }
}

