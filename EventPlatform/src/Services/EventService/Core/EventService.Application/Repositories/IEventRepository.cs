using EventService.Application.UseCases.GetAllEvents;
using EventService.Domain.Entities;

namespace EventService.Application.Repositories;

public interface IEventRepository
{
    /// <summary>Adds the event (and its zones) to the current unit of work. Does not commit.</summary>
    Task CreateAsync(EventDomain domain, CancellationToken cancellationToken);

    /// <summary>Returns events matching the given criteria (all of them when criteria is empty).</summary>
    Task<List<EventDomain>> SearchAsync(EventSearchCriteria criteria, CancellationToken cancellationToken);

    Task<EventDomain?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);
}
