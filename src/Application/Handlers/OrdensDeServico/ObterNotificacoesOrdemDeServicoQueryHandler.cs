using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Handlers.OrdensDeServico;

public sealed class ObterNotificacoesOrdemDeServicoQueryHandler : OrdemDeServicoHandlerBase, IRequestHandler<ObterNotificacoesOrdemDeServicoQuery, IEnumerable<NotificacaoClienteDto>>
{
    public ObterNotificacoesOrdemDeServicoQueryHandler(OrdemDeServicoHandlerDependencies dependencies)
        : base(dependencies)
    {
    }

    public Task<IEnumerable<NotificacaoClienteDto>> Handle(ObterNotificacoesOrdemDeServicoQuery query, CancellationToken cancellationToken)
    {
        return ObterNotificacoesAsync(query.Id, cancellationToken);
    }
}

