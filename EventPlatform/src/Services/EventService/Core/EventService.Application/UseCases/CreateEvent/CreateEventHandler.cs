using AutoMapper;
using EventService.Application.Abstractions;
using EventService.Application.Common;
using EventService.Application.Repositories;
using EventService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.UseCases.CreateEvent;

/// <summary>
/// Creates an event together with its zones in a single transaction and publishes the
/// <c>EventCreated</c> integration message through the transactional outbox. The list
/// cache is invalidated so the next GET reflects the new event.
/// </summary>
public sealed class CreateEventHandler : IRequestHandler<CreateEventRequest, CreateEventResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventRepository _eventRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEventHandler> _logger;

    public CreateEventHandler(
        IUnitOfWork unitOfWork,
        IEventRepository eventRepository,
        IEventPublisher eventPublisher,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _eventRepository = eventRepository;
        _eventPublisher = eventPublisher;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CreateEventResponse> Handle(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var zones = request.Zones.Select(z => new ZoneDomain(z.Name, z.Price, z.Capacity));
        var domain = EventDomain.CreateNew(request.Name, request.EventDate, request.Place, zones);

        var correlationId = Guid.NewGuid();

        // 1) Stage the aggregate in the unit of work.
        await _eventRepository.CreateAsync(domain, cancellationToken);

        // 2) Stage the integration message in the outbox (same DB transaction).
        await _eventPublisher.PublishEventCreatedAsync(domain, correlationId, cancellationToken);

        // 3) Commit data + outbox atomically. The broker delivery happens after commit.
        await _unitOfWork.CommitAsync(cancellationToken);

        // 4) Best-effort cache invalidation (fail-open): drop the full list and roll the
        //    search generation so every cached advanced-search result is invalidated too.
        await _cache.RemoveAsync(EventCacheKeys.AllEvents, cancellationToken);
        await _cache.SetAsync(
            EventCacheKeys.SearchGeneration,
            Guid.NewGuid().ToString("N"),
            TimeSpan.FromDays(1),
            cancellationToken);

        _logger.LogInformation(
            "Event {EventId} created with {ZoneCount} zones (correlationId {CorrelationId}).",
            domain.EventId, domain.Zones.Count, correlationId);

        return new CreateEventResponse
        {
            Event = _mapper.Map<EventDto>(domain),
            CorrelationId = correlationId
        };
    }
}
