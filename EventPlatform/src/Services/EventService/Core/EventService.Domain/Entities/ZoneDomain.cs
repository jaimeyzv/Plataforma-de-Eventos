namespace EventService.Domain.Entities;

/// <summary>
/// A zone (sector) inside an event: a name, a price and a finite capacity.
/// Capacity is the basis for concurrency control against overselling.
/// </summary>
public sealed class ZoneDomain
{
    public Guid ZoneId { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }

    public ZoneDomain()
    {
    }

    public ZoneDomain(string name, decimal price, int capacity)
    {
        ZoneId = Guid.NewGuid();
        Name = name;
        Price = price;
        Capacity = capacity;
    }

    /// <summary>Invariants enforced at the domain level (defense in depth vs. the API validation).</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Zone name is required.");

        if (Price < 0)
            throw new ArgumentException($"Zone '{Name}' price cannot be negative.");

        if (Capacity <= 0)
            throw new ArgumentException($"Zone '{Name}' capacity must be greater than zero.");
    }
}
