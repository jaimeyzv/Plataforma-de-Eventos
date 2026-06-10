namespace EventPlatform.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published to the message broker when an event is created.
/// The shape of this message is part of the public contract between microservices
/// and matches the schema required by the challenge:
/// { messageId, eventId, name, occurredAt, correlationId, version }.
/// </summary>
public sealed record EventCreatedIntegrationEvent
{
    /// <summary>Unique id of THIS message. Used by consumers for idempotency.</summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>Id of the aggregate (event) that originated the message.</summary>
    public Guid EventId { get; init; }

    /// <summary>Human readable name of the event.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>UTC instant (ISO-8601) when the domain fact occurred.</summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Correlation id used to trace a single business flow across services.</summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>Schema version, allows backward/forward compatible evolution.</summary>
    public int Version { get; init; } = 1;
}
