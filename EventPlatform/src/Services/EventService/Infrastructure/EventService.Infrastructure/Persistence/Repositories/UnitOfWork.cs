using EventService.Application.Repositories;
using EventService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Commits domain changes and outbox messages atomically. Wrapped in an EF Core
/// execution strategy so it cooperates with SQL Server transient-fault retries
/// (<c>EnableRetryOnFailure</c>) — a key resilience pattern under high load.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly EventDbContext _context;

    public UnitOfWork(EventDbContext context) => _context = context;

    public async Task<bool> CommitAsync(CancellationToken cancellationToken)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var changes = await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return changes > 0;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }, cancellationToken);
    }
}
