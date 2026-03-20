using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IOrdemDeServicoRepository
{
    Task<OrdemDeServico?> ObterPorIdAsync(Guid id);
    Task<OrdemDeServico?> ObterPorNumeroAsync(string numero);
    Task<IEnumerable<OrdemDeServico>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<OrdemDeServico>> ObterPorStatusAsync(StatusOrdemDeServico status);
    Task<IEnumerable<OrdemDeServico>> ObterTodosAsync();
    Task<OrdemDeServico> CriarAsync(OrdemDeServico ordem);
    Task<OrdemDeServico> AtualizarAsync(OrdemDeServico ordem);
    Task DeletarAsync(Guid id);
}
