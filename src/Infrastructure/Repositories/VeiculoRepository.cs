using Microsoft.EntityFrameworkCore;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;
using oficina_mecanica.Infrastructure.Data;

namespace oficina_mecanica.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly OficinaDbContext _context;

    public VeiculoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Veiculo?> ObterPorIdAsync(Guid id)
    {
        return await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Veiculo?> ObterPorPlacaAsync(string placa)
    {
        return await _context.Veiculos.FirstOrDefaultAsync(v => v.Placa == placa);
    }

    public async Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId)
    {
        return await _context.Veiculos.Where(v => v.ClienteId == clienteId).ToListAsync();
    }

    public async Task<IEnumerable<Veiculo>> ObterTodosAsync()
    {
        return await _context.Veiculos.ToListAsync();
    }

    public async Task<Veiculo> CriarAsync(Veiculo veiculo)
    {
        _context.Veiculos.Add(veiculo);
        await _context.SaveChangesAsync();
        return veiculo;
    }

    public async Task<Veiculo> AtualizarAsync(Veiculo veiculo)
    {
        _context.Veiculos.Update(veiculo);
        await _context.SaveChangesAsync();
        return veiculo;
    }

    public async Task DeletarAsync(Guid id)
    {
        var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id);
        if (veiculo != null)
        {
            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
        }
    }
}
