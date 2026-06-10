using MediatR;

namespace EventService.Application.UseCases.GetAllEvents;

public sealed class GetAllEventsRequest : IRequest<GetAllEventsResponse>
{
    /// <summary>Optional advanced-search filters. Empty means "list all events".</summary>
    public EventSearchCriteria Criteria { get; init; } = new();
}
