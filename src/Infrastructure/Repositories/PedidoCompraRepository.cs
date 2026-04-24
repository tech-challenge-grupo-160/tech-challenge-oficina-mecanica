using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Domain.Enums;
using Fiap.TechChallenge.OficinaMecanica.Domain.Repositories;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class PedidoCompraRepository : IPedidoCompraRepository
{
    private readonly OficinaDbContext _context;

    public PedidoCompraRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<PedidoCompra?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Set<PedidoCompra>()
            .Include(x => x.Peca)
            .Include(x => x.OrdemDeServico)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PedidoCompra?> ObterPendentePorOrdemEPecaAsync(int ordemDeServicoId, int pecaId, CancellationToken cancellationToken)
    {
        return await _context.Set<PedidoCompra>()
            .FirstOrDefaultAsync(
                x => x.OrdemDeServicoId == ordemDeServicoId &&
                     x.PecaId == pecaId &&
                     x.Status == StatusPedidoCompra.Pendente,
                cancellationToken);
    }

    public async Task<int> ContarAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<PedidoCompra>().CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<PedidoCompra>> ObterPaginadoAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _context.Set<PedidoCompra>()
            .Include(x => x.Peca)
            .OrderByDescending(x => x.DataSolicitacao)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PedidoCompra>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        return await _context.Set<PedidoCompra>()
            .Include(x => x.Peca)
            .Where(x => x.OrdemDeServicoId == ordemDeServicoId)
            .OrderByDescending(x => x.DataSolicitacao)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PedidoCompra> CriarAsync(PedidoCompra pedidoCompra, CancellationToken cancellationToken)
    {
        _context.Set<PedidoCompra>().Add(pedidoCompra);
        await _context.SaveChangesAsync(cancellationToken);
        return pedidoCompra;
    }

    public async Task<PedidoCompra> AtualizarAsync(PedidoCompra pedidoCompra, CancellationToken cancellationToken)
    {
        _context.Set<PedidoCompra>().Update(pedidoCompra);
        await _context.SaveChangesAsync(cancellationToken);
        return pedidoCompra;
    }
}
