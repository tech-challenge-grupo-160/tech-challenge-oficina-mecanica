using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterHistoricoOrdemDeServicoQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ObterHistoricoOrdemDeServicoQuery, IEnumerable<OrdemServicoHistoricoDto>>
{
    public ObterHistoricoOrdemDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<IEnumerable<OrdemServicoHistoricoDto>> Handle(ObterHistoricoOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterHistoricoAsync(query.Id, cancellationToken);
    }
}

