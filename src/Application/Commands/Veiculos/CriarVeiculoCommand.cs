using Fiap.TechChallenge.OficinaMecanica.Application.Results.Veiculos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Veiculos;

public sealed class CriarVeiculoCommand : IRequest<VeiculoResult>
{
    public string Placa { get; init; } = null!;
    public string Marca { get; init; } = null!;
    public string Modelo { get; init; } = null!;
    public int Ano { get; init; }
    public string CpfCnpj { get; init; } = null!;
}
