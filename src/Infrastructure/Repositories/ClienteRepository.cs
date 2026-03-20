using Microsoft.EntityFrameworkCore;
using oficina_mecanica.Domain.Entities;
using oficina_mecanica.Domain.Repositories;
using oficina_mecanica.Infrastructure.Data;

namespace oficina_mecanica.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly OficinaDbContext _context;

    public ClienteRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.CpfCnpj == cpfCnpj);
    }

    public async Task<IEnumerable<Cliente>> ObterTodosAsync()
    {
        return await _context.Clientes.ToListAsync();
    }

    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> AtualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task DeletarAsync(Guid id)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }
    }
}
