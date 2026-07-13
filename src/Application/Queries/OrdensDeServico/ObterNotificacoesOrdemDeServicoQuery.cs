using Fiap.TechChallenge.OficinaMecanica.Application.Results.OrdensDeServico;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterNotificacoesOrdemDeServicoQuery : IRequest<IEnumerable<NotificacaoClienteResult>>
{
    public int Id { get; init; }
}

