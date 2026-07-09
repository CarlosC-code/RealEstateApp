using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Offer;
using RealEstateApp.Core.Application.ViewModels.Offer;

namespace RealEstateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class OfferDtoMappingProfile : Profile
    {
        public OfferDtoMappingProfile()
        {
            CreateMap<OfferDto, OfferViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.OfferDate, opt => opt.Ignore());

            CreateMap<OfferDto, SaveOfferViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.OfferDate, opt => opt.Ignore());
        }
    }
}