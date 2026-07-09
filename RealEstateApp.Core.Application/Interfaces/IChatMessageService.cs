using RealEstateApp.Core.Application.Dtos.Chat;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IChatMessageService : IGenericService<ChatMessageDto>
    {
        Task<List<ChatMessageDto>> GetByPropertyAndUsersAsync(int propertyId,
            string senderId, string receiverId);
        Task<ChatMessageDto?> SendMessageAsync(int propertyId, string senderId,
            string receiverId, string message);
        Task<List<ChatMessageDto>> GetConversationsByAgentAsync(string agentId);
    }
}