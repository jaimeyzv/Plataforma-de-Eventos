using NotificationService.Domain.Types;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Trace of a notification produced in response to an integration event.
/// Persisted in MongoDB (document store) for auditability.
/// </summary>
public sealed class NotificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public Guid EventId { get; set; }
    public Guid CorrelationId { get; set; }
    public string Channel { get; set; } = "Email";
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int Version { get; set; } = 1;
}
