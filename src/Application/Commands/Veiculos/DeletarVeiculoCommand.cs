using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;

public sealed class DeletarVeiculoCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
