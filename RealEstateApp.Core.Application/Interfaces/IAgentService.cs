using RealEstateApp.Core.Application.Dtos.Agent;
using RealEstateApp.Core.Application.Dtos.Property;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IAgentService
    {
        Task<List<AgentDto>> GetAllAsync();
        Task<AgentDto?> GetByIdAsync(string id);
        Task<List<AgentDto>> SearchByNameAsync(string name);
        Task<string?> UpdateProfileAsync(string agentId, string firstName, string lastName, string phone, string? photoUrl);
        Task<string?> GetClientNameAsync(string clientId);
        Task<List<PropertyDto>> GetAgentPropertiesAsync(string agentId);
        Task ChangeStatusAsync(string agentId, bool isActive);
    }
}