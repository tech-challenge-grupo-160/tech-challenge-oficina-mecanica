using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;

public sealed class ListarServicosQuery : IRequest<IEnumerable<ServicoResult>>
{
}
