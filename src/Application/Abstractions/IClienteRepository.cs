using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Application.abstractions;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Cliente?> ObterPorCpfCnpjAsync(Documento cpfCnpj, CancellationToken cancellationToken);
    Task<IEnumerable<Cliente>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Cliente>> ObterPaginadoAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken);
    Task<int> ContarAsync(string? nome, string? cpfCnpj, CancellationToken cancellationToken);
    Task<Cliente> CriarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task<Cliente> AtualizarAsync(Cliente cliente, CancellationToken cancellationToken);
    Task DeletarAsync(int id, CancellationToken cancellationToken);
}
