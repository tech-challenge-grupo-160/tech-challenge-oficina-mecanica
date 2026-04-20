using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<Servico>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Servico> CriarAsync(Servico servico, CancellationToken cancellationToken);
    Task<Servico> AtualizarAsync(Servico servico, CancellationToken cancellationToken);
    Task DeletarAsync(int id, CancellationToken cancellationToken);
}
