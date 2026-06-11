using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class AdicionarPecaAOrdemCommand : IRequest<OrdemDeServicoDto>
{
    public int OrdemDeServicoId { get; init; }
    public int PecaId { get; init; }
    public int Quantidade { get; init; }
}

