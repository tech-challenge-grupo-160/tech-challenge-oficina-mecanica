using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterHistoricoOrdemDeServicoQuery : IRequest<IEnumerable<OrdemServicoHistoricoDto>>
{
    public int Id { get; init; }
}

