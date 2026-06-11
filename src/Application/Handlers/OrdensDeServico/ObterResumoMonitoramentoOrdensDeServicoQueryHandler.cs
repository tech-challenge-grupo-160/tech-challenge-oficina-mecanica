using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterResumoMonitoramentoOrdensDeServicoQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ObterResumoMonitoramentoOrdensDeServicoQuery, ResumoMonitoramentoOrdensDeServicoDto>
{
    public ObterResumoMonitoramentoOrdensDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<ResumoMonitoramentoOrdensDeServicoDto> Handle(ObterResumoMonitoramentoOrdensDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterResumoMonitoramentoAsync(query.Page, query.PageSize, cancellationToken);
    }
}

