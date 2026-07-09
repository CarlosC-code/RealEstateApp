using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class PropertyDtoMappingProfile : Profile
    {
        public PropertyDtoMappingProfile()
        {
            CreateMap<PropertyDto, PropertyViewModel>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Improvements, opt => opt.MapFrom(src => src.Improvements))
                .ReverseMap()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Improvements, opt => opt.Ignore())
                .ForMember(dest => dest.ImprovementIds, opt => opt.Ignore());

            CreateMap<PropertyDto, SavePropertyViewModel>()
                .ForMember(dest => dest.ImprovementIds, opt => opt.MapFrom(src => src.ImprovementIds))
                .ReverseMap()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Improvements, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.SaleTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.AgentName, opt => opt.Ignore());
        }
    }
}