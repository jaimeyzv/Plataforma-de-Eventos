using AutoMapper;
using EventService.Application.Repositories;
using EventService.Application.UseCases.GetAllEvents;
using EventService.Domain.Entities;
using EventService.Infrastructure.Persistence.Context;
using EventService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly EventDbContext _context;
    private readonly IMapper _mapper;

    public EventRepository(EventDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task CreateAsync(EventDomain domain, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<EventEntity>(domain);
        _context.Events.Add(entity);
        return Task.CompletedTask;
    }

    public async Task<List<EventDomain>> SearchAsync(EventSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var query = _context.Events
            .Include(e => e.Zones)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.Status))
        {
            var status = criteria.Status.Trim();
            query = query.Where(e => e.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Text))
        {
            var like = $"%{criteria.Text.Trim()}%";
            query = query.Where(e =>
                EF.Functions.Like(e.Name, like) || EF.Functions.Like(e.Place, like));
        }

        if (criteria.From is { } from)
            query = query.Where(e => e.EventDate >= from);

        if (criteria.To is { } to)
            query = query.Where(e => e.EventDate <= to);

        if (criteria.MaxPrice is { } maxPrice)
            query = query.Where(e => e.Zones.Any(z => z.Price <= maxPrice));

        var entities = await query
            .OrderByDescending(e => e.EventDate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EventDomain>>(entities);
    }

    public async Task<EventDomain?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var entity = await _context.Events
            .Include(e => e.Zones)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);

        return entity is null ? null : _mapper.Map<EventDomain>(entity);
    }
}
