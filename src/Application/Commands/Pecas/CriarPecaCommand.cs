using Fiap.TechChallenge.OficinaMecanica.Application.Results.Pecas;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Pecas;

public sealed class CriarPecaCommand : IRequest<PecaResult>
{
    public string Nome { get; init; } = null!;
    public string Marca { get; init; } = null!;
    public string Modelo { get; init; } = null!;
    public decimal Preco { get; init; }
    public int QuantidadeEstoque { get; init; }
}
