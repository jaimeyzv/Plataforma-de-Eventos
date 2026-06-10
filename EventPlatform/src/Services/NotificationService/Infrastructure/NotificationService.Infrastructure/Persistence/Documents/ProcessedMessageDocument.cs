using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Infrastructure.Persistence.Documents;

/// <summary>
/// Idempotency record. The messageId is the document <c>_id</c>, so a duplicate insert
/// fails with a duplicate-key error — the natural, atomic idempotency guarantee.
/// </summary>
public sealed class ProcessedMessageDocument
{
    [BsonId]
    public Guid MessageId { get; set; }

    public DateTime ReservedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAtUtc { get; set; }

    public string Status { get; set; } = "Reserved";
}
