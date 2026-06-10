using EventService.Application.Common;

namespace EventService.Application.UseCases.CreateEvent;

public sealed record CreateEventResponse
{
    public EventDto Event { get; init; } = default!;

    /// <summary>Correlation id of the integration flow triggered by this creation.</summary>
    public Guid CorrelationId { get; init; }
}
