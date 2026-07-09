using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Chat;
using RealEstateApp.Core.Application.ViewModels.Chat;

namespace RealEstateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class ChatMessageDtoMappingProfile : Profile
    {
        public ChatMessageDtoMappingProfile()
        {
            CreateMap<ChatMessageDto, ChatMessageViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.SentAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsFromClient, opt => opt.Ignore());

            CreateMap<ChatMessageDto, SaveChatMessageViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.SentAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsFromClient, opt => opt.Ignore());
        }
    }
}