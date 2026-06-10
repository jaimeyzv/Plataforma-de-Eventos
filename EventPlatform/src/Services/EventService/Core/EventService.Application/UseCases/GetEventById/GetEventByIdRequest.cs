using EventService.Application.Common;
using MediatR;

namespace EventService.Application.UseCases.GetEventById;

public sealed class GetEventByIdRequest : IRequest<EventDto?>
{
    public Guid EventId { get; init; }

    public GetEventByIdRequest(Guid eventId) => EventId = eventId;
}
