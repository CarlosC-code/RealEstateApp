using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class PropertyMappingProfile : Profile
    {
        public PropertyMappingProfile()
        {
            CreateMap<Property, PropertyDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Images, o => o.MapFrom(s => s.Images != null
                    ? s.Images.Select(i => i.ImageUrl!).ToList()
                    : new List<string>()))
                .ForMember(d => d.Improvements, o => o.MapFrom(s => s.PropertyImprovements != null
                    ? s.PropertyImprovements.Select(pi => pi.Improvement!.Name!).ToList()
                    : new List<string>()))
                .ForMember(d => d.ImprovementIds, o => o.MapFrom(s => s.PropertyImprovements != null
                    ? s.PropertyImprovements.Select(pi => pi.ImprovementId).ToList()
                    : new List<int>()))
                .ReverseMap()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyImprovements, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
                .ForMember(dest => dest.SaleType, opt => opt.Ignore());
        }
    }
}