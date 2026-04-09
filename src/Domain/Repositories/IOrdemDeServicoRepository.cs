using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IOrdemDeServicoRepository
{
    Task<OrdemDeServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OrdemDeServico?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServico>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServico>> ObterPorStatusAsync(StatusOrdemDeServico status, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServico>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<OrdemDeServico> CriarAsync(OrdemDeServico ordem, CancellationToken cancellationToken);
    Task<OrdemDeServico> AtualizarAsync(OrdemDeServico ordem, CancellationToken cancellationToken);
    Task DeletarAsync(Guid id, CancellationToken cancellationToken);
}
