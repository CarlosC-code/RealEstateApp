using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Chat;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class ChatMessageMappingProfile : Profile
    {
        public ChatMessageMappingProfile()
        {
            CreateMap<ChatMessage, ChatMessageDto>()
                .ReverseMap()
                .ForMember(dest => dest.Property, opt => opt.Ignore());
        }
    }
}