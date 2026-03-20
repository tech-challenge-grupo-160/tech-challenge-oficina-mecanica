using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Servico>> ObterTodosAsync();
    Task<Servico> CriarAsync(Servico servico);
    Task<Servico> AtualizarAsync(Servico servico);
    Task DeletarAsync(Guid id);
}
