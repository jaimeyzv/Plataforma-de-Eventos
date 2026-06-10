using EventPlatform.Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.UseCases.ProcessEventCreated;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// MassTransit consumer for <see cref="EventCreatedIntegrationEvent"/>. Delegates to the
/// application processor, which enforces idempotency by messageId.
/// </summary>
public sealed class EventCreatedConsumer : IConsumer<EventCreatedIntegrationEvent>
{
    private readonly EventCreatedProcessor _processor;
    private readonly ILogger<EventCreatedConsumer> _logger;

    public EventCreatedConsumer(EventCreatedProcessor processor, ILogger<EventCreatedConsumer> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received EventCreated messageId={MessageId} eventId={EventId} v{Version}.",
            message.MessageId, message.EventId, message.Version);

        await _processor.ProcessAsync(message, context.CancellationToken);
    }
}
