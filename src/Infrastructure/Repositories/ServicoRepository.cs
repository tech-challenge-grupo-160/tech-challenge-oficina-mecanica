using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class ServicoRepository : IServicoRepository
{
    private readonly OficinaDbContext _context;

    public ServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Servicos.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Servico>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        return await _context.Servicos.ToListAsync(cancellationToken);
    }

    public async Task<Servico> CriarAsync(Servico servico, CancellationToken cancellationToken)
    {
        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync(cancellationToken);
        return servico;
    }

    public async Task<Servico> AtualizarAsync(Servico servico, CancellationToken cancellationToken)
    {
        _context.Servicos.Update(servico);
        await _context.SaveChangesAsync(cancellationToken);
        return servico;
    }

    public async Task DeletarAsync(Guid id, CancellationToken cancellationToken)
    {
        var servico = await _context.Servicos.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (servico != null)
        {
            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
