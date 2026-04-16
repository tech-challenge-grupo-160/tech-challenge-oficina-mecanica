using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Veiculo?> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken);
    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<Veiculo>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Veiculo> CriarAsync(Veiculo veiculo, CancellationToken cancellationToken);
    Task<Veiculo> AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken);
    Task DeletarAsync(int id, CancellationToken cancellationToken);
}
