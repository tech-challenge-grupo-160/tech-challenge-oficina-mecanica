using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;

public sealed class CriarServicoCommand : IRequest<ServicoResult>
{
    public string Nome { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public decimal Preco { get; init; }
    public int TempoEstimado { get; init; }
}
