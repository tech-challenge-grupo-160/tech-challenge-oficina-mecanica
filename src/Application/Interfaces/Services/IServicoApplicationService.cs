using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

public interface IServicoApplicationService
{
    Task<ServicoDto> CriarServicoAsync(CriarServicoDto dto, CancellationToken cancellationToken);
    Task<ServicoDto> ObterServicoAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<ServicoDto>> ListarServicosAsync(CancellationToken cancellationToken);
    Task<ServicoDto> AtualizarServicoAsync(int id, AtualizarServicoDto dto, CancellationToken cancellationToken);
    Task DeletarServicoAsync(int id, CancellationToken cancellationToken);
}
