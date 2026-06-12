using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class RemoverServicoDaOrdemCommand : IRequest<OrdemDeServicoResult>
{
    public int OrdemDeServicoId { get; init; }
    public int ServicoId { get; init; }
}

