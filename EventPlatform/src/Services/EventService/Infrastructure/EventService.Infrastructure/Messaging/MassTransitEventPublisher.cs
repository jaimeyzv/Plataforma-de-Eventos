using EventPlatform.Contracts.IntegrationEvents;
using EventService.Application.Abstractions;
using EventService.Domain.Entities;
using MassTransit;

namespace EventService.Infrastructure.Messaging;

/// <summary>
/// Publishes integration events through MassTransit. Because the bus is configured with
/// the Entity Framework <c>UseBusOutbox</c>, calling <see cref="IPublishEndpoint.Publish"/>
/// here only stages the message in the outbox table; it is dispatched to RabbitMQ after the
/// surrounding transaction commits, giving at-least-once delivery without losing messages.
/// </summary>
public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public Task PublishEventCreatedAsync(EventDomain @event, Guid correlationId, CancellationToken cancellationToken)
    {
        var message = new EventCreatedIntegrationEvent
        {
            MessageId = Guid.NewGuid(),
            EventId = @event.EventId,
            Name = @event.Name,
            OccurredAt = DateTimeOffset.UtcNow,
            CorrelationId = correlationId,
            Version = 1
        };

        return _publishEndpoint.Publish(message, ctx => ctx.CorrelationId = correlationId, cancellationToken);
    }
}
