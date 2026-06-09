using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

public sealed class ObterVeiculoPorIdQuery : IRequest<VeiculoDto>
{
    public int Id { get; init; }
}
