using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Pecas;

public sealed class ListarPecasQuery : IRequest<IEnumerable<PecaResult>>
{
}
