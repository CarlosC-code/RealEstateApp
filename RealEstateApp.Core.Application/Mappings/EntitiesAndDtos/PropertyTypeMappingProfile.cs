using AutoMapper;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class PropertyTypeMappingProfile : Profile
    {
        public PropertyTypeMappingProfile()
        {
            CreateMap<PropertyType, PropertyTypeDto>()
                .ReverseMap();
        }
    }
}