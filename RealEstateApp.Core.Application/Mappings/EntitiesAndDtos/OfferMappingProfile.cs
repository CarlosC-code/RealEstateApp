using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Offer;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class OfferMappingProfile : Profile
    {
        public OfferMappingProfile()
        {
            CreateMap<Offer, OfferDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Property, opt => opt.Ignore());
        }
    }
}