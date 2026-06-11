using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterMonitoramentoOrdemDeServicoQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ObterMonitoramentoOrdemDeServicoQuery, MonitoramentoOrdemDeServicoDto>
{
    public ObterMonitoramentoOrdemDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<MonitoramentoOrdemDeServicoDto> Handle(ObterMonitoramentoOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterMonitoramentoAsync(query.Id, cancellationToken);
    }
}

