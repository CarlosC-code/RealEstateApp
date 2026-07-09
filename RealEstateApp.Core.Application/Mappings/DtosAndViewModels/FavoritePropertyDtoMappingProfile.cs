using AutoMapper;
using RealEstateApp.Core.Application.Dtos.FavoriteProperty;
using RealEstateApp.Core.Application.ViewModels.FavoriteProperty;

namespace RealEstateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class FavoritePropertyDtoMappingProfile : Profile
    {
        public FavoritePropertyDtoMappingProfile()
        {
            CreateMap<FavoritePropertyDto, FavoritePropertyViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.PropertyCode, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore());

            CreateMap<FavoritePropertyDto, SaveFavoritePropertyViewModel>()
                .ReverseMap();
        }
    }
}