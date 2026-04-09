using Microsoft.EntityFrameworkCore;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;
using oficina_mecanica.Infrastructure.Data;

namespace oficina_mecanica.Infrastructure.Repositories;

public class OrdemDeServicoRepository : IOrdemDeServicoRepository
{
    private readonly OficinaDbContext _context;

    public OrdemDeServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<OrdemDeServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<OrdemDeServico?> ObterPorNumeroAsync(string numero, CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .FirstOrDefaultAsync(o => o.Numero == numero, cancellationToken);
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterPorClienteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico
            .Where(o => o.ClienteId == clienteId)
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterPorStatusAsync(StatusOrdemDeServico status, CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico
            .Where(o => o.Status == status)
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrdemDeServico> CriarAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        _context.OrdensDeServico.Add(ordem);
        await _context.SaveChangesAsync(cancellationToken);
        return ordem;
    }

    public async Task<OrdemDeServico> AtualizarAsync(OrdemDeServico ordem, CancellationToken cancellationToken)
    {
        _context.OrdensDeServico.Update(ordem);
        await _context.SaveChangesAsync(cancellationToken);
        return ordem;
    }

    public async Task DeletarAsync(Guid id, CancellationToken cancellationToken)
    {
        var ordem = await _context.OrdensDeServico.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (ordem != null)
        {
            _context.OrdensDeServico.Remove(ordem);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
