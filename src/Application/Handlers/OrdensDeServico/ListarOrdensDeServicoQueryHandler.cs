using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ListarOrdensDeServicoQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ListarOrdensDeServicoQuery, PagedResultDto<OrdemDeServicoDto>>
{
    public ListarOrdensDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<PagedResultDto<OrdemDeServicoDto>> Handle(ListarOrdensDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ListarOrdensDeServicoAsync(
            query.Page,
            query.PageSize,
            query.ClienteId,
            query.Status,
            query.Numero,
            query.DataAberturaInicio,
            query.DataAberturaFim,
            cancellationToken);
    }
}

