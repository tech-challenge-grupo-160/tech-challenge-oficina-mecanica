using Microsoft.EntityFrameworkCore;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;
using oficina_mecanica.Infrastructure.Data;

namespace oficina_mecanica.Infrastructure.Repositories;

public class ServicoRepository : IServicoRepository
{
    private readonly OficinaDbContext _context;

    public ServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Servico?> ObterPorIdAsync(Guid id)
    {
        return await _context.Servicos.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Servico>> ObterTodosAsync()
    {
        return await _context.Servicos.ToListAsync();
    }

    public async Task<Servico> CriarAsync(Servico servico)
    {
        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync();
        return servico;
    }

    public async Task<Servico> AtualizarAsync(Servico servico)
    {
        _context.Servicos.Update(servico);
        await _context.SaveChangesAsync();
        return servico;
    }

    public async Task DeletarAsync(Guid id)
    {
        var servico = await _context.Servicos.FirstOrDefaultAsync(s => s.Id == id);
        if (servico != null)
        {
            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();
        }
    }
}
