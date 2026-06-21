using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface INotificacaoClienteRepository
{
    Task<NotificacaoCliente> CriarAsync(NotificacaoCliente notificacao, CancellationToken cancellationToken);
    Task<IEnumerable<NotificacaoCliente>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken);
}
