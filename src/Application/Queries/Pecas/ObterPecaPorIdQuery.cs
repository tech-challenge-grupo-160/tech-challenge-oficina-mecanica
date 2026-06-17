using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;

public sealed class ObterPecaPorIdQuery : IRequest<PecaResult>
{
    public int Id { get; init; }
}
