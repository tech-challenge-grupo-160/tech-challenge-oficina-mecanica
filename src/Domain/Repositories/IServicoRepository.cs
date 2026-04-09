using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Servico>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Servico> CriarAsync(Servico servico, CancellationToken cancellationToken);
    Task<Servico> AtualizarAsync(Servico servico, CancellationToken cancellationToken);
    Task DeletarAsync(Guid id, CancellationToken cancellationToken);
}
