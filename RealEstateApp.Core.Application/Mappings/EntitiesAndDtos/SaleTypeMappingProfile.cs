using AutoMapper;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class SaleTypeMappingProfile : Profile
    {
        public SaleTypeMappingProfile()
        {
            CreateMap<SaleType, SaleTypeDto>()
                .ReverseMap();
        }
    }
}