using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;

public sealed class DeletarPecaCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
