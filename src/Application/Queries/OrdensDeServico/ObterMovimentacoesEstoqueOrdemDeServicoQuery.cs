using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterMovimentacoesEstoqueOrdemDeServicoQuery : IRequest<IEnumerable<MovimentacoesEstoquePorPecaDto>>
{
    public int Id { get; init; }
}

