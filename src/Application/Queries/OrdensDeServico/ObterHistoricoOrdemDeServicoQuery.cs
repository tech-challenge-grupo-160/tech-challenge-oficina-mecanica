using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterHistoricoOrdemDeServicoQuery : IRequest<IEnumerable<OrdemServicoHistoricoResult>>
{
    public int Id { get; init; }
}

