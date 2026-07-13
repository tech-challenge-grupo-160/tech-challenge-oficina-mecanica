using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class PecaRepository : IPecaRepository
{
    private readonly OficinaDbContext _context;

    public PecaRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteEmOrdemDeServicoAtivaAsync(int pecaId, CancellationToken cancellationToken)
    {
        return await _context.OrdemDeServicoPecas
            .AnyAsync(
                item => item.PecaId == pecaId,
                cancellationToken);
    }

    public async Task<Peca?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Pecas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Peca>> ObterPorIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        return await _context.Pecas
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Peca>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        return await _context.Pecas.ToListAsync(cancellationToken);
    }

    public async Task<Peca> CriarAsync(Peca peca, CancellationToken cancellationToken)
    {
        _context.Pecas.Add(peca);
        await _context.SaveChangesAsync(cancellationToken);
        return peca;
    }

    public async Task<Peca> AtualizarAsync(Peca peca, CancellationToken cancellationToken)
    {
        _context.Pecas.Update(peca);
        await _context.SaveChangesAsync(cancellationToken);
        return peca;
    }

    public async Task DeletarAsync(int id, CancellationToken cancellationToken)
    {
        var peca = await _context.Pecas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (peca != null)
        {
            _context.Pecas.Remove(peca);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
