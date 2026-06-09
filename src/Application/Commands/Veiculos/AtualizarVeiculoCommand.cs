using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;

public sealed class AtualizarVeiculoCommand : IRequest<VeiculoDto>
{
    public int Id { get; init; }
    public string Marca { get; init; } = null!;
    public string Modelo { get; init; } = null!;
    public int Ano { get; init; }
}
