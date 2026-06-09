using Fiap.TechChallenge.OficinaMecanica.Application.Results.Servicos;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Commands.Servicos;

public sealed class AtualizarServicoCommand : IRequest<ServicoResult>
{
    public int Id { get; init; }
    public string Nome { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public decimal Preco { get; init; }
    public int TempoEstimado { get; init; }
}
