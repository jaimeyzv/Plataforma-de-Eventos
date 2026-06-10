namespace NotificationService.Application.Abstractions;

/// <summary>
/// Idempotency gate. Backed by a MongoDB collection with a unique index on the messageId,
/// so a message is processed at most once even under concurrent/duplicate delivery.
/// </summary>
public interface IProcessedMessageStore
{
    /// <summary>
    /// Atomically reserves the messageId. Returns <c>true</c> if this caller won the
    /// reservation (first time seen) or <c>false</c> if it was already processed/in-flight.
    /// </summary>
    Task<bool> TryReserveAsync(Guid messageId, CancellationToken cancellationToken);

    /// <summary>Releases a reservation so the message can be retried (used on failure).</summary>
    Task ReleaseAsync(Guid messageId, CancellationToken cancellationToken);

    /// <summary>Marks a reserved message as successfully processed.</summary>
    Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken);
}
