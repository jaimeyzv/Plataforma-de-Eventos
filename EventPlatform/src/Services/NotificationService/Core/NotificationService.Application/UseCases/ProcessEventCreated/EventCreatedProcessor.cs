using EventPlatform.Contracts.IntegrationEvents;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Types;

namespace NotificationService.Application.UseCases.ProcessEventCreated;

/// <summary>
/// Application service that reacts to an <see cref="EventCreatedIntegrationEvent"/>: it
/// guarantees idempotency by messageId, sends the notification and stores an audit log.
/// </summary>
public sealed class EventCreatedProcessor
{
    private readonly IProcessedMessageStore _processedMessages;
    private readonly INotificationLogRepository _logRepository;
    private readonly INotificationSender _sender;
    private readonly ILogger<EventCreatedProcessor> _logger;

    public EventCreatedProcessor(
        IProcessedMessageStore processedMessages,
        INotificationLogRepository logRepository,
        INotificationSender sender,
        ILogger<EventCreatedProcessor> logger)
    {
        _processedMessages = processedMessages;
        _logRepository = logRepository;
        _sender = sender;
        _logger = logger;
    }

    public async Task ProcessAsync(EventCreatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        // 1) Idempotency: reserve the messageId. If we don't win it, it was already handled.
        var reserved = await _processedMessages.TryReserveAsync(message.MessageId, cancellationToken);
        if (!reserved)
        {
            _logger.LogInformation(
                "Duplicate message {MessageId} for event {EventId} ignored (idempotent).",
                message.MessageId, message.EventId);
            return;
        }

        try
        {
            var notification = new NotificationLog
            {
                MessageId = message.MessageId,
                EventId = message.EventId,
                CorrelationId = message.CorrelationId,
                Channel = "Email",
                Recipient = "subscribers@eventplatform.local",
                Subject = $"New event published: {message.Name}",
                Body = $"The event '{message.Name}' (id {message.EventId}) has been published.",
                OccurredAt = message.OccurredAt,
                Version = message.Version,
                Status = NotificationStatus.Pending
            };

            await _sender.SendAsync(notification, cancellationToken);
            notification.Status = NotificationStatus.Sent;

            await _logRepository.AddAsync(notification, cancellationToken);
            await _processedMessages.MarkProcessedAsync(message.MessageId, cancellationToken);

            _logger.LogInformation(
                "Processed EventCreated {MessageId} for event {EventId} (correlationId {CorrelationId}).",
                message.MessageId, message.EventId, message.CorrelationId);
        }
        catch (Exception ex)
        {
            // Release the reservation so MassTransit's retry can re-deliver and reprocess.
            await _processedMessages.ReleaseAsync(message.MessageId, cancellationToken);
            _logger.LogError(ex, "Failed to process message {MessageId}; reservation released for retry.", message.MessageId);
            throw;
        }
    }
}
