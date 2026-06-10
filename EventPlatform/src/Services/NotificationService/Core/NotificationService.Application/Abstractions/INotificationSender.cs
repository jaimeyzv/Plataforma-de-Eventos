using NotificationService.Domain.Entities;

namespace NotificationService.Application.Abstractions;

/// <summary>
/// Delivers a notification through a channel (email/SMS/push). The default
/// implementation simulates delivery and is MailKit-ready for real SMTP.
/// </summary>
public interface INotificationSender
{
    Task SendAsync(NotificationLog notification, CancellationToken cancellationToken);
}
