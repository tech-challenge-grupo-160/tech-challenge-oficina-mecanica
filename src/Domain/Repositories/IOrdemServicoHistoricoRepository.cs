using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;

namespace Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;

public interface IOrdemServicoHistoricoRepository
{
    Task<OrdemServicoHistorico> CriarAsync(OrdemServicoHistorico historico, CancellationToken cancellationToken);
    Task<IEnumerable<OrdemServicoHistorico>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken);
}
