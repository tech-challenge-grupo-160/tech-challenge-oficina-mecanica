using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

public interface IPecaApplicationService
{
    Task<PecaDto> CriarPecaAsync(CriarPecaDto dto, CancellationToken cancellationToken);
    Task<PecaDto> ObterPecaAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<PecaDto>> ListarPecasAsync(CancellationToken cancellationToken);
    Task<PecaDto> AtualizarPecaAsync(int id, AtualizarPecaDto dto, CancellationToken cancellationToken);
    Task DeletarPecaAsync(int id, CancellationToken cancellationToken);
}
