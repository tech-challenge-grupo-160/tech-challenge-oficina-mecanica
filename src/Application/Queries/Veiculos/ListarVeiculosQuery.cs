using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

public sealed class ListarVeiculosQuery : IRequest<IEnumerable<VeiculoDto>>
{
}
