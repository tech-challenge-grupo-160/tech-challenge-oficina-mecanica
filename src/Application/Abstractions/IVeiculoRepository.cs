using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.ValueObjects;

namespace Fiap.TechChallenge.OficinaMecanica.Application.abstractions;

public interface IVeiculoRepository
{
    Task<bool> ExisteEmOrdemDeServicoAtivaAsync(int veiculoId, CancellationToken cancellationToken);
    Task<Veiculo?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Veiculo?> ObterPorPlacaAsync(PlacaVeiculo placa, CancellationToken cancellationToken);
    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<bool> ExistePorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<Veiculo>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Veiculo> CriarAsync(Veiculo veiculo, CancellationToken cancellationToken);
    Task<Veiculo> AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken);
    Task DeletarAsync(int id, CancellationToken cancellationToken);
}
