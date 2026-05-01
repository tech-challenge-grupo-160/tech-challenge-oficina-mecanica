using Microsoft.EntityFrameworkCore.Storage;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Application.Services;

public class EfTransactionManager : ITransactionManager
{
    private readonly OficinaDbContext _context;

    public EfTransactionManager(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        if (_context.Database.IsInMemory())
        {
            return await action(cancellationToken);
        }

        await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (_context.Database.IsInMemory())
        {
            await action(cancellationToken);
            return;
        }

        await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
