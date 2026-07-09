using AutoMapper;
using RealEstateApp.Core.Application.Dtos.SaleType;
using RealEstateApp.Core.Application.ViewModels.SaleType;

namespace RealEstateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class SaleTypeDtoMappingProfile : Profile
    {
        public SaleTypeDtoMappingProfile()
        {
            CreateMap<SaleTypeDto, SaleTypeViewModel>()
                .ReverseMap();

            CreateMap<SaleTypeDto, SaveSaleTypeViewModel>()
                .ReverseMap();
        }
    }
}