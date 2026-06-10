using AutoMapper;
using EventService.Domain.Entities;
using EventService.Domain.Types;
using EventService.Infrastructure.Persistence.Entities;

namespace EventService.Infrastructure.Persistence.Mappers;

/// <summary>Maps between EF persistence entities and rich domain models.</summary>
public sealed class EventEntityMapper : Profile
{
    public EventEntityMapper()
    {
        CreateMap<ZoneEntity, ZoneDomain>().ReverseMap();

        CreateMap<EventEntity, EventDomain>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => Enum.Parse<EventStatus>(src.Status)));

        CreateMap<EventDomain, EventEntity>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }
}
