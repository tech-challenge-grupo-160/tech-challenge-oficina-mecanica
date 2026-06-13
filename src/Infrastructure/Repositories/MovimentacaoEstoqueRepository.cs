using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
{
    private readonly OficinaDbContext _context;

    public MovimentacaoEstoqueRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<MovimentacaoEstoque> CriarAsync(MovimentacaoEstoque movimentacao, CancellationToken cancellationToken)
    {
        _context.Set<MovimentacaoEstoque>().Add(movimentacao);
        await _context.SaveChangesAsync(cancellationToken);
        return movimentacao;
    }

    public async Task<IEnumerable<MovimentacaoEstoque>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        return await _context.Set<MovimentacaoEstoque>()
            .Include(x => x.Peca)
            .Where(x => x.OrdemDeServicoId == ordemDeServicoId)
            .OrderByDescending(x => x.DataMovimentacao)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
