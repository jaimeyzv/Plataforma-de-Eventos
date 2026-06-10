using AutoMapper;
using EventService.Domain.Entities;

namespace EventService.Application.Common;

/// <summary>Maps rich domain models to the DTOs returned by the API.</summary>
public sealed class EventMappingProfile : Profile
{
    public EventMappingProfile()
    {
        CreateMap<ZoneDomain, ZoneDto>();

        CreateMap<EventDomain, EventDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
