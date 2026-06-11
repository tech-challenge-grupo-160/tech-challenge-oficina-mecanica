using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterOrdemDeServicoPorIdQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ObterOrdemDeServicoPorIdQuery, OrdemDeServicoDto>
{
    public ObterOrdemDeServicoPorIdQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<OrdemDeServicoDto> Handle(ObterOrdemDeServicoPorIdQuery query, CancellationToken cancellationToken)
    {
        return ObterOrdemDeServicoAsync(query.Id, cancellationToken);
    }
}

