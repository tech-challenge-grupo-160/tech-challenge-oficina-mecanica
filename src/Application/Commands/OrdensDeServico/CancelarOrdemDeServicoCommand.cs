using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.OrdensDeServico;

public sealed class CancelarOrdemDeServicoCommand : IRequest<OrdemDeServicoDto>
{
    public int Id { get; init; }
    public string MotivoCancelamento { get; init; } = null!;
}

