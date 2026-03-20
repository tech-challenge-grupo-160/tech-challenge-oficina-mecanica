using oficina_mecanica.Domain.Entities;

namespace oficina_mecanica.Domain.Repositories;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id);
    Task<Veiculo?> ObterPorPlacaAsync(string placa);
    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<Veiculo>> ObterTodosAsync();
    Task<Veiculo> CriarAsync(Veiculo veiculo);
    Task<Veiculo> AtualizarAsync(Veiculo veiculo);
    Task DeletarAsync(Guid id);
}
