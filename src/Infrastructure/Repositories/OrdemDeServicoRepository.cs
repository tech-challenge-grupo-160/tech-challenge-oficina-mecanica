using Microsoft.EntityFrameworkCore;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class OrdemDeServicoRepository : IOrdemDeServicoRepository
{
    private readonly OficinaDbContext _context;

    public OrdemDeServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<OrdemDeServico?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
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

    public async Task<IEnumerable<OrdemDeServico>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico
            .Where(o => o.ClienteId == clienteId)
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico.AnyAsync(o => o.ClienteId == clienteId, cancellationToken);
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterPorStatusAsync(StatusOrdemDeServico status, CancellationToken cancellationToken)
    {
        return await _context.OrdensDeServico
            .Where(o => o.Status == status)
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(
        int? clienteId,
        StatusOrdemDeServico? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken)
    {
        var query = AplicarFiltros(
            _context.OrdensDeServico.AsNoTracking(),
            clienteId,
            status,
            numero,
            dataAberturaInicio,
            dataAberturaFim);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrdemDeServico>> ObterPaginadoAsync(
        int page,
        int pageSize,
        int? clienteId,
        StatusOrdemDeServico? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim,
        CancellationToken cancellationToken)
    {
        var query = AplicarFiltros(
                _context.OrdensDeServico,
                clienteId,
                status,
                numero,
                dataAberturaInicio,
                dataAberturaFim)
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .OrderByDescending(o => o.DataAbertura)
            .ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
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

    public async Task DeletarAsync(int id, CancellationToken cancellationToken)
    {
        var ordem = await _context.OrdensDeServico.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (ordem != null)
        {
            _context.OrdensDeServico.Remove(ordem);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static IQueryable<OrdemDeServico> AplicarFiltros(
        IQueryable<OrdemDeServico> query,
        int? clienteId,
        StatusOrdemDeServico? status,
        string? numero,
        DateTime? dataAberturaInicio,
        DateTime? dataAberturaFim)
    {
        if (clienteId.HasValue)
        {
            query = query.Where(o => o.ClienteId == clienteId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(numero))
        {
            query = query.Where(o => o.Numero == numero);
        }

        if (dataAberturaInicio.HasValue)
        {
            query = query.Where(o => o.DataAbertura >= dataAberturaInicio.Value);
        }

        if (dataAberturaFim.HasValue)
        {
            query = query.Where(o => o.DataAbertura <= dataAberturaFim.Value);
        }

        return query;
    }
}
