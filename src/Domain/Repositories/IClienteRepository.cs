using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
    Task<IEnumerable<Cliente>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Cliente> CriarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task<Cliente> AtualizarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task DeletarAsync(Guid id, CancellationToken cancellationToken);
}
