namespace EventService.Infrastructure.Persistence.Entities;

/// <summary>EF Core persistence model for a zone.</summary>
public class ZoneEntity
{
    public Guid ZoneId { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }

    public EventEntity? Event { get; set; }
}
