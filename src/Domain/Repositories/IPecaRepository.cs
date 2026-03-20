using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Peca>> ObterTodosAsync();
    Task<Peca> CriarAsync(Peca peca);
    Task<Peca> AtualizarAsync(Peca peca);
    Task DeletarAsync(Guid id);
}
