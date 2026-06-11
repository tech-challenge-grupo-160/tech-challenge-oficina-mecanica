using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterResumoMonitoramentoOrdensDeServicoQuery : IRequest<ResumoMonitoramentoOrdensDeServicoDto>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

