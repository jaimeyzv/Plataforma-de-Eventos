namespace EventService.Application.Repositories;

/// <summary>
/// Commits all pending changes inside a single database transaction.
/// When the transactional outbox is enabled, the integration messages produced
/// during the unit of work are persisted atomically together with the data.
/// </summary>
public interface IUnitOfWork
{
    Task<bool> CommitAsync(CancellationToken cancellationToken);
}
