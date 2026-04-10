using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Veiculo?> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken);
    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<Veiculo>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Veiculo> CriarAsync(Veiculo veiculo, CancellationToken cancellationToken);
    Task<Veiculo> AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken);
    Task DeletarAsync(Guid id, CancellationToken cancellationToken);
}
