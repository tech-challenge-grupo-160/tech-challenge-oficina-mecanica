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

    public async Task<OrdemDeServico?> ObterPorIdAsync(Guid id)
    {
        return await _context.OrdensDeServico
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OrdemDeServico?> ObterPorNumeroAsync(string numero)
    {
        return await _context.OrdensDeServico
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .FirstOrDefaultAsync(o => o.Numero == numero);
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterPorClienteAsync(Guid clienteId)
    {
        return await _context.OrdensDeServico
            .Where(o => o.ClienteId == clienteId)
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterPorStatusAsync(StatusOrdemDeServico status)
    {
        return await _context.OrdensDeServico
            .Where(o => o.Status == status)
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterTodosAsync()
    {
        return await _context.OrdensDeServico
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync();
    }

    public async Task<OrdemDeServico> CriarAsync(OrdemDeServico ordem)
    {
        _context.OrdensDeServico.Add(ordem);
        await _context.SaveChangesAsync();
        return ordem;
    }

    public async Task<OrdemDeServico> AtualizarAsync(OrdemDeServico ordem)
    {
        _context.OrdensDeServico.Update(ordem);
        await _context.SaveChangesAsync();
        return ordem;
    }

    public async Task DeletarAsync(Guid id)
    {
        var ordem = await _context.OrdensDeServico.FirstOrDefaultAsync(o => o.Id == id);
        if (ordem != null)
        {
            _context.OrdensDeServico.Remove(ordem);
            await _context.SaveChangesAsync();
        }
    }
}
