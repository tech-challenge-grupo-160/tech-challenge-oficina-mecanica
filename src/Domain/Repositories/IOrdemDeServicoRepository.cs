using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

public interface IOrdemDeServicoRepository
{
    Task<OrdemDeServico?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<OrdemDeServico?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServico>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<bool> ExistePorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServico>> ObterPorStatusAsync(StatusOrdemDeServico status, CancellationToken cancellationToken);
    Task<int> ContarAsync(
        int? clienteId,
        StatusOrdemDeServico? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken);
    Task<IEnumerable<OrdemDeServico>> ObterPaginadoAsync(
        int page,
        int pageSize,
        int? clienteId,
        StatusOrdemDeServico? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken);
    Task<OrdemDeServico> CriarAsync(OrdemDeServico ordem, CancellationToken cancellationToken);
    Task<OrdemDeServico> AtualizarAsync(OrdemDeServico ordem, CancellationToken cancellationToken);
    Task DeletarAsync(int id, CancellationToken cancellationToken);
}
