using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

public interface IVeiculoApplicationService
{
    Task<VeiculoDto> CriarVeiculoAsync(CriarVeiculoDto dto, CancellationToken cancellationToken);
    Task<VeiculoDto> CriarVeiculoParaClienteAsync(string cpfCnpj, CriarVeiculoParaClienteDto dto, CancellationToken cancellationToken);
    Task<VeiculoDto> ObterVeiculoAsync(int id, CancellationToken cancellationToken);
    Task<VeiculoDto> ObterVeiculoPorPlacaAsync(string placa, CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosAsync(CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosPorClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IEnumerable<VeiculoDto>> ListarVeiculosPorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken);
    Task<VeiculoDto> AtualizarVeiculoAsync(int id, AtualizarVeiculoDto dto, CancellationToken cancellationToken);
    Task DeletarVeiculoAsync(int id, CancellationToken cancellationToken);
}
