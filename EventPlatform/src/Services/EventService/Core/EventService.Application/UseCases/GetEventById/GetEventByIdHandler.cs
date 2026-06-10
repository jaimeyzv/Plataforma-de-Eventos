using AutoMapper;
using EventService.Application.Abstractions;
using EventService.Application.Common;
using EventService.Application.Repositories;
using MediatR;

namespace EventService.Application.UseCases.GetEventById;

public sealed class GetEventByIdHandler : IRequestHandler<GetEventByIdRequest, EventDto?>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;

    public GetEventByIdHandler(IEventRepository eventRepository, ICacheService cache, IMapper mapper)
    {
        _eventRepository = eventRepository;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<EventDto?> Handle(GetEventByIdRequest request, CancellationToken cancellationToken)
    {
        var cacheKey = EventCacheKeys.EventById(request.EventId);

        var cached = await _cache.GetAsync<EventDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var domain = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (domain is null)
            return null;

        var dto = _mapper.Map<EventDto>(domain);
        await _cache.SetAsync(cacheKey, dto, CacheTtl, cancellationToken);
        return dto;
    }
}
