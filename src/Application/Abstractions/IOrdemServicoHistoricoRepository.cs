using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;

public interface IOrdemServicoHistoricoRepository
{
    Task<OrdemServicoHistorico> CriarAsync(OrdemServicoHistorico historico, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServicoHistorico>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken);
}
