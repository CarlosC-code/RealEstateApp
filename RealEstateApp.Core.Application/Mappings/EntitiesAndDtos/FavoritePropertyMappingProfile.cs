using AutoMapper;
using RealEstateApp.Core.Application.Dtos.FavoriteProperty;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class FavoritePropertyMappingProfile : Profile
    {
        public FavoritePropertyMappingProfile()
        {
            CreateMap<FavoriteProperty, FavoritePropertyDto>()
                .ReverseMap()
                .ForMember(dest => dest.Property, opt => opt.Ignore());
        }
    }
}