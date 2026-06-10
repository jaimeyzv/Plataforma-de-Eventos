namespace EventService.Application.Common;

public sealed record ZoneDto
{
    public Guid ZoneId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Capacity { get; init; }
}
