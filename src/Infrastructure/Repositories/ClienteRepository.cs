using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly OficinaDbContext _context;

    public ClienteRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
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

    public async Task<IEnumerable<Cliente>> ObterPaginadoAsync(int page, int pageSize, string? nome, string? cpfCnpj, CancellationToken cancellationToken)
    {
        return await AplicarFiltros(nome, cpfCnpj)
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(string? nome, string? cpfCnpj, CancellationToken cancellationToken)
    {
        return await AplicarFiltros(nome, cpfCnpj).CountAsync(cancellationToken);
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

    public async Task DeletarAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private IQueryable<Cliente> AplicarFiltros(string? nome, string? cpfCnpj)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var nomeNormalizado = nome.Trim().ToLower();
            query = query.Where(c => c.Nome.ToLower().Contains(nomeNormalizado));
        }

        if (!string.IsNullOrWhiteSpace(cpfCnpj))
        {
            var documentoNormalizado = cpfCnpj.Trim();
            query = query.Where(c => c.CpfCnpj.Contains(documentoNormalizado));
        }

        return query;
    }
}
