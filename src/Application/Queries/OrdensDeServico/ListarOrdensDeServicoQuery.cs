using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ListarOrdensDeServicoQuery : IRequest<PagedResultDto<OrdemDeServicoResult>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? ClienteId { get; init; }
    public string? Status { get; init; }
    public string? Numero { get; init; }
    public DateTime? DataAberturaInicio { get; init; }
    public DateTime? DataAberturaFim { get; init; }
}

