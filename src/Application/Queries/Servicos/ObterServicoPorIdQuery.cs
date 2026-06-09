using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Servicos;

public sealed class ObterServicoPorIdQuery : IRequest<ServicoResult>
{
    public int Id { get; init; }
}
