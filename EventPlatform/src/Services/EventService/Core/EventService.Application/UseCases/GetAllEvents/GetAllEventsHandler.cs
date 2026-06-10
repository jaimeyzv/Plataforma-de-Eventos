using AutoMapper;
using EventService.Application.Abstractions;
using EventService.Application.Common;
using EventService.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.UseCases.GetAllEvents;

/// <summary>
/// Returns all events. Read-through Redis cache: the response is served from cache
/// when present and re-populated from SQL on a miss. Cache failures fail-open.
/// </summary>
public sealed class GetAllEventsHandler : IRequestHandler<GetAllEventsRequest, GetAllEventsResponse>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllEventsHandler> _logger;

    public GetAllEventsHandler(
        IEventRepository eventRepository,
        ICacheService cache,
        IMapper mapper,
        ILogger<GetAllEventsHandler> logger)
    {
        _eventRepository = eventRepository;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetAllEventsResponse> Handle(GetAllEventsRequest request, CancellationToken cancellationToken)
    {
        var criteria = request.Criteria;
        var cacheKey = await ResolveCacheKeyAsync(criteria, cancellationToken);

        var cached = await _cache.GetAsync<GetAllEventsResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("GET /events served from cache ({CacheKey}).", cacheKey);
            return cached;
        }

        var domainList = await _eventRepository.SearchAsync(criteria, cancellationToken);
        var response = new GetAllEventsResponse
        {
            Events = _mapper.Map<List<EventDto>>(domainList)
        };

        await _cache.SetAsync(cacheKey, response, CacheTtl, cancellationToken);
        return response;
    }

    /// <summary>
    /// An empty search reuses the explicitly-invalidated "all events" key. Filtered searches
    /// get a per-criteria key namespaced by the current search generation, so a write that
    /// rolls the generation invalidates every cached search at once.
    /// </summary>
    private async Task<string> ResolveCacheKeyAsync(EventSearchCriteria criteria, CancellationToken cancellationToken)
    {
        if (criteria.IsEmpty)
            return EventCacheKeys.AllEvents;

        var generation = await _cache.GetAsync<string>(EventCacheKeys.SearchGeneration, cancellationToken) ?? "0";
        return EventCacheKeys.Search(generation, criteria.CacheToken());
    }
}
