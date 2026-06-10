using EventService.Domain.Entities;

namespace EventService.Application.Abstractions;

/// <summary>
/// Publishes integration events to the message broker. The implementation uses the
/// transactional outbox, so the publish is captured inside the same DB transaction
/// as the data change and only dispatched after a successful commit.
/// </summary>
public interface IEventPublisher
{
    Task PublishEventCreatedAsync(EventDomain @event, Guid correlationId, CancellationToken cancellationToken);
}
