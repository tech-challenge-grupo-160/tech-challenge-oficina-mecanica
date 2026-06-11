using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class AdicionarServicoAOrdemCommand : IRequest<OrdemDeServicoDto>
{
    public int OrdemDeServicoId { get; init; }
    public int ServicoId { get; init; }
}

