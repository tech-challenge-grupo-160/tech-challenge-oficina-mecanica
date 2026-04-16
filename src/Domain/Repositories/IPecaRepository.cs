using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<Peca>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Peca> CriarAsync(Peca peca, CancellationToken cancellationToken);
    Task<Peca> AtualizarAsync(Peca peca, CancellationToken cancellationToken);
    Task DeletarAsync(int id, CancellationToken cancellationToken);
}
