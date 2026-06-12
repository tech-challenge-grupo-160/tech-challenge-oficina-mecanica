using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterOrdemDeServicoPorIdQuery : IRequest<OrdemDeServicoResult>
{
    public int Id { get; init; }
}

