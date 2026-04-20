using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly OficinaDbContext _context;

    public VeiculoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Veiculo?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Veiculo?> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken)
    {
        return await _context.Veiculos.FirstOrDefaultAsync(v => v.Placa == placa, cancellationToken);
    }

    public async Task<IEnumerable<Veiculo>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        return await _context.Veiculos.Where(v => v.ClienteId == clienteId).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        return await _context.Veiculos.AnyAsync(v => v.ClienteId == clienteId, cancellationToken);
    }

    public async Task<IEnumerable<Veiculo>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        return await _context.Veiculos.ToListAsync(cancellationToken);
    }

    public async Task<Veiculo> CriarAsync(Veiculo veiculo, CancellationToken cancellationToken)
    {
        _context.Veiculos.Add(veiculo);
        await _context.SaveChangesAsync(cancellationToken);
        return veiculo;
    }

    public async Task<Veiculo> AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken)
    {
        _context.Veiculos.Update(veiculo);
        await _context.SaveChangesAsync(cancellationToken);
        return veiculo;
    }

    public async Task DeletarAsync(int id, CancellationToken cancellationToken)
    {
        var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (veiculo != null)
        {
            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
