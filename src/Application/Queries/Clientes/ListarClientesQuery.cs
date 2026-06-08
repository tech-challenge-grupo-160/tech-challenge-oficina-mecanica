using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Results.Clientes;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Clientes;

public sealed class ListarClientesQuery : IRequest<PagedResultDto<ClienteResult>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public string? Nome { get; init; }
    public string? CpfCnpj { get; init; }
}
