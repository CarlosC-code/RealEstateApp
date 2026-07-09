using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Improvement;
using RealEstateApp.Core.Application.ViewModels.Improvement;

namespace RealEstateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class ImprovementDtoMappingProfile : Profile
    {
        public ImprovementDtoMappingProfile()
        {
            CreateMap<ImprovementDto, ImprovementViewModel>()
                .ReverseMap();

            CreateMap<ImprovementDto, SaveImprovementViewModel>()
                .ReverseMap();
        }
    }
}