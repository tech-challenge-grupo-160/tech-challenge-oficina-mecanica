using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id);
    Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj);
    Task<IEnumerable<Cliente>> ObterTodosAsync();
    Task<Cliente> CriarAsync(Cliente cliente);
    Task<Cliente> AtualizarAsync(Cliente cliente);
    Task DeletarAsync(Guid id);
}
