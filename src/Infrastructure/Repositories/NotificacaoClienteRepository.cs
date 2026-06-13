using Fiap.TechChallenge.OficinaMecanica.Application.abstractions;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class NotificacaoClienteRepository : INotificacaoClienteRepository
{
    private readonly OficinaDbContext _context;

    public NotificacaoClienteRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<NotificacaoCliente> CriarAsync(NotificacaoCliente notificacao, CancellationToken cancellationToken)
    {
        _context.Set<NotificacaoCliente>().Add(notificacao);
        await _context.SaveChangesAsync(cancellationToken);
        return notificacao;
    }

    public async Task<IEnumerable<NotificacaoCliente>> ObterPorOrdemDeServicoAsync(int ordemDeServicoId, CancellationToken cancellationToken)
    {
        return await _context.Set<NotificacaoCliente>()
            .AsNoTracking()
            .Where(x => x.OrdemDeServicoId == ordemDeServicoId)
            .OrderBy(x => x.DataNotificacao)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
