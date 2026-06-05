using Fiap.TechChallenge.OficinaMecanica.Application.DTOs;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Interfaces.Services;

public interface IAcompanhamentoOSApplicationService
{
    Task<AcompanhamentoOrdemDeServicoDto> ObterStatusAsync(string codigo, string token, CancellationToken cancellationToken);
}
