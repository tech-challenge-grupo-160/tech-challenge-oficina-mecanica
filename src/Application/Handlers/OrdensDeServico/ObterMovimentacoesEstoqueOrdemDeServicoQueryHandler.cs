using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ObterMovimentacoesEstoqueOrdemDeServicoQuery, IEnumerable<MovimentacoesEstoquePorPecaDto>>
{
    public ObterMovimentacoesEstoqueOrdemDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<IEnumerable<MovimentacoesEstoquePorPecaDto>> Handle(ObterMovimentacoesEstoqueOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterMovimentacoesEstoqueAsync(query.Id, cancellationToken);
    }
}

