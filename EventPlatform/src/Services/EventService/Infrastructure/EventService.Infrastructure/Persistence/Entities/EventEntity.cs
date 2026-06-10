namespace EventService.Infrastructure.Persistence.Entities;

/// <summary>EF Core persistence model for an event (kept separate from the domain model).</summary>
public class EventEntity
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset EventDate { get; set; }
    public string Place { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    /// <summary>SQL Server rowversion used as the optimistic concurrency token.</summary>
    public byte[]? RowVersion { get; set; }

    public ICollection<ZoneEntity> Zones { get; set; } = new List<ZoneEntity>();
}
