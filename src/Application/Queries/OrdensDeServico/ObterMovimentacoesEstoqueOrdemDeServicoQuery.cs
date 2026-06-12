using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterMovimentacoesEstoqueOrdemDeServicoQuery : IRequest<IEnumerable<MovimentacoesEstoquePorPecaResult>>
{
    public int Id { get; init; }
}

