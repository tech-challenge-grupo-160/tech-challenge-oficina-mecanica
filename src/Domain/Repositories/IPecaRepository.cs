using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Peca>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Peca> CriarAsync(Peca peca, CancellationToken cancellationToken);
    Task<Peca> AtualizarAsync(Peca peca, CancellationToken cancellationToken);
    Task DeletarAsync(Guid id, CancellationToken cancellationToken);
}
