using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

public sealed class ObterVeiculoPorIdQuery : IRequest<VeiculoResult>
{
    public int Id { get; init; }
}
