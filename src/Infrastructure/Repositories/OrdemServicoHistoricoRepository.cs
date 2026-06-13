using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class OrdemServicoHistoricoRepository : IOrdemServicoHistoricoRepository
{
    private readonly OficinaDbContext _context;

    public OrdemServicoHistoricoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<OrdemServicoHistorico> CriarAsync(OrdemServicoHistorico historico, CancellationToken cancellationToken)
    {
        _context.Set<OrdemServicoHistorico>().Add(historico);
        await _context.SaveChangesAsync(cancellationToken);
        return historico;
    }

    public async Task<IEnumerable<OrdemServicoHistorico>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        return await _context.Set<OrdemServicoHistorico>()
            .AsNoTracking()
            .Where(h => h.OrdemDeServicoId == ordemDeServicoId)
            .OrderBy(h => h.DataEvento)
            .ThenBy(h => h.Id)
            .ToListAsync(cancellationToken);
    }
}
