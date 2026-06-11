using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;
using MediatR;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Queries.OrdensDeServico;

public sealed class ObterNotificacoesOrdemDeServicoQuery : IRequest<IEnumerable<NotificacaoClienteDto>>
{
    public int Id { get; init; }
}

