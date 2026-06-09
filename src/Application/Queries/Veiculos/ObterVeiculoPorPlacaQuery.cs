using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.Veiculos;

public sealed class ObterVeiculoPorPlacaQuery : IRequest<VeiculoResult>
{
    public string Placa { get; init; } = null!;
}
