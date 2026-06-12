using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterResumoMonitoramentoOrdensDeServicoQuery : IRequest<ResumoMonitoramentoOrdensDeServicoResult>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

