using Microsoft.EntityFrameworkCore;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;
using oficina_mecanica.Infrastructure.Data;

namespace oficina_mecanica.Infrastructure.Repositories;

public class PecaRepository : IPecaRepository
{
    private readonly OficinaDbContext _context;

    public PecaRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Pecas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
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

    public async Task DeletarAsync(Guid id, CancellationToken cancellationToken)
    {
        var peca = await _context.Pecas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (peca != null)
        {
            _context.Pecas.Remove(peca);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
