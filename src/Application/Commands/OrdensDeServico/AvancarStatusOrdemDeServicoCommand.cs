using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class AvancarStatusOrdemDeServicoCommand : IRequest<OrdemDeServicoResult>
{
    public string Numero { get; init; } = string.Empty;
}
