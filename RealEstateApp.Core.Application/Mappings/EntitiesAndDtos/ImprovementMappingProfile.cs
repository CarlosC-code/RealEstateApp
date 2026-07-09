using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class ImprovementMappingProfile : Profile
    {
        public ImprovementMappingProfile()
        {
            CreateMap<Improvement, ImprovementDto>()
                .ReverseMap();
        }
    }
}