using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ListarOrdensDeServicoQuery : IRequest<PagedResultDto<OrdemDeServicoDto>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? ClienteId { get; init; }
    public string? Status { get; init; }
    public string? Numero { get; init; }
    public DateTime? DataAberturaInicio { get; init; }
    public DateTime? DataAberturaFim { get; init; }
}

