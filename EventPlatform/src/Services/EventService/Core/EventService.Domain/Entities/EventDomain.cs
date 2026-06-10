using EventService.Domain.Types;

namespace EventService.Domain.Entities;

/// <summary>
/// Aggregate root for an event. Owns its zones and guards its own invariants.
/// </summary>
public sealed class EventDomain
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset EventDate { get; set; }
    public string Place { get; set; } = string.Empty;
    public EventStatus Status { get; set; }

    /// <summary>Optimistic concurrency token. Surfaced as SQL rowversion.</summary>
    public byte[]? RowVersion { get; set; }

    public List<ZoneDomain> Zones { get; set; } = new();

    /// <summary>
    /// Factory that builds a brand new, validated event together with its zones in a
    /// single consistent operation (matches the "create event + zones in one transaction" rule).
    /// </summary>
    public static EventDomain CreateNew(string name, DateTimeOffset eventDate, string place, IEnumerable<ZoneDomain> zones)
    {
        var domain = new EventDomain
        {
            EventId = Guid.NewGuid(),
            Name = name,
            EventDate = eventDate,
            Place = place,
            Status = EventStatus.Published,
            Zones = zones?.ToList() ?? new List<ZoneDomain>()
        };

        domain.Validate();
        return domain;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Event name is required.");

        if (string.IsNullOrWhiteSpace(Place))
            throw new ArgumentException("Event place is required.");

        if (EventDate == default)
            throw new ArgumentException("Event date is required.");

        if (Zones is null || Zones.Count == 0)
            throw new ArgumentException("At least one zone is required.");

        foreach (var zone in Zones)
        {
            zone.EventId = EventId;
            zone.Validate();
        }
    }

    public int TotalCapacity => Zones?.Sum(z => z.Capacity) ?? 0;
}
