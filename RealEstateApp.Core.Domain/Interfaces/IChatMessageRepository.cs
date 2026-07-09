using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Domain.Interfaces
{
    public interface IChatMessageRepository : IGenericRepository<ChatMessage>
    {
        Task<List<ChatMessage>> GetByPropertyAndUsersAsync(int propertyId,
            string senderId, string receiverId);
        Task<List<ChatMessage>> GetByAgentIdAsync(string agentId);
    }
}