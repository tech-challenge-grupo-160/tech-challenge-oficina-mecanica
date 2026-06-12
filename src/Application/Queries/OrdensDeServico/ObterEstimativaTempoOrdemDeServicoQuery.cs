using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterEstimativaTempoOrdemDeServicoQuery : IRequest<EstimativaTempoOrdemDeServicoResult>
{
    public int Id { get; init; }
}

