using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterOrdemDeServicoPorIdQuery : IRequest<OrdemDeServicoDto>
{
    public int Id { get; init; }
}

