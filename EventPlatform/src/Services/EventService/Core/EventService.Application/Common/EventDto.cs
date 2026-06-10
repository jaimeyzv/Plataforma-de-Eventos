namespace EventService.Application.Common;

public sealed record EventDto
{
    public Guid EventId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset EventDate { get; init; }
    public string Place { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<ZoneDto> Zones { get; init; } = Array.Empty<ZoneDto>();
}
