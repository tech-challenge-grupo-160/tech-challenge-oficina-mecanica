using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;

public sealed class DeletarServicoCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
