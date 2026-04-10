using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly OficinaDbContext _context;

    public ClienteRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj, CancellationToken cancellationToken)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.CpfCnpj == cpfCnpj, cancellationToken);
    }

    public async Task<IEnumerable<Cliente>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        return await _context.Clientes.ToListAsync(cancellationToken);
    }

    public async Task<Cliente> CriarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync(cancellationToken);
        return cliente;
    }

    public async Task<Cliente> AtualizarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync(cancellationToken);
        return cliente;
    }

    public async Task DeletarAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
