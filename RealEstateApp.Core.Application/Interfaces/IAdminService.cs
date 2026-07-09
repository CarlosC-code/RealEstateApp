using RealEstateApp.Core.Application.Dtos.Admin;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IAdminService
    {
        // Dashboard
        Task<DashboardDto> GetDashboardAsync();

        // Agentes
        Task<List<AgentListDto>> GetAllAgentsAsync();
        Task ToggleAgentStatusAsync(string agentId);
        Task DeleteAgentAsync(string agentId);

        // Administradores
        Task<List<AdminListDto>> GetAllAdminsAsync();
        Task<RegisterAdminDto?> GetAdminByIdAsync(string id);
        Task<string?> CreateAdminAsync(RegisterAdminDto dto);
        Task<string?> UpdateAdminAsync(string id, RegisterAdminDto dto);
        Task ToggleAdminStatusAsync(string adminId);

        // Desarrolladores
        Task<List<DeveloperListDto>> GetAllDevelopersAsync();
        Task<RegisterDeveloperDto?> GetDeveloperByIdAsync(string id);
        Task<string?> CreateDeveloperAsync(RegisterDeveloperDto dto);
        Task<string?> UpdateDeveloperAsync(string id, RegisterDeveloperDto dto);
        Task ToggleDeveloperStatusAsync(string developerId);
        Task<List<ClientListDto>> GetAllClientsAsync();
        Task ToggleClientStatusAsync(string clientId);
    }
}