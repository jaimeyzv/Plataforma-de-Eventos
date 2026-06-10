using EventService.Application.Common;

namespace EventService.Application.UseCases.GetAllEvents;

public sealed record GetAllEventsResponse
{
    public IReadOnlyList<EventDto> Events { get; init; } = Array.Empty<EventDto>();
}
