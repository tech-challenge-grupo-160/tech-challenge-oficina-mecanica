using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterEstimativaTempoOrdemDeServicoQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ObterEstimativaTempoOrdemDeServicoQuery, EstimativaTempoOrdemDeServicoDto>
{
    public ObterEstimativaTempoOrdemDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<EstimativaTempoOrdemDeServicoDto> Handle(ObterEstimativaTempoOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterEstimativaTempoAsync(query.Id, cancellationToken);
    }
}

