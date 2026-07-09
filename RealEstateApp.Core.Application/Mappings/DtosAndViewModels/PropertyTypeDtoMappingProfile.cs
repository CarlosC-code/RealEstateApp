using AutoMapper;
using RealEstateApp.Core.Application.Dtos.PropertyType;
using RealEstateApp.Core.Application.ViewModels.PropertyType;

namespace RealEstateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class PropertyTypeDtoMappingProfile : Profile
    {
        public PropertyTypeDtoMappingProfile()
        {
            CreateMap<PropertyTypeDto, PropertyTypeViewModel>()
                .ReverseMap();

            CreateMap<PropertyTypeDto, SavePropertyTypeViewModel>()
                .ReverseMap();
        }
    }
}