using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

public sealed class ListarVeiculosPorClienteQuery : IRequest<IEnumerable<VeiculoResult>>
{
    public int ClienteId { get; init; }
}
