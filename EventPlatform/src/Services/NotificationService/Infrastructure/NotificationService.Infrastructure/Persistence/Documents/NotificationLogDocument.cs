using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Infrastructure.Persistence.Documents;

public sealed class NotificationLogDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }
    public Guid EventId { get; set; }
    public Guid CorrelationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int Version { get; set; }
}
