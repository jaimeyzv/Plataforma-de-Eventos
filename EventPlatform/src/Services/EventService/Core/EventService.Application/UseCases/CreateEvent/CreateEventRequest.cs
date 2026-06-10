using MediatR;

namespace EventService.Application.UseCases.CreateEvent;

public sealed class CreateEventRequest : IRequest<CreateEventResponse>
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset EventDate { get; set; }
    public string Place { get; set; } = string.Empty;
    public List<CreateEventZone> Zones { get; set; } = new();
}

public sealed class CreateEventZone
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }
}
