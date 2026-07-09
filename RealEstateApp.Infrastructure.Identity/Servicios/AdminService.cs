using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.Admin;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserManager<ApplicationUser> userManager,
            IPropertyRepository propertyRepository,
            ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _propertyRepository = propertyRepository;
            _logger = logger;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            try
            {
                var properties = await _propertyRepository.GetAllAsync();
                var agents = await _userManager.GetUsersInRoleAsync("Agent");
                var clients = await _userManager.GetUsersInRoleAsync("Client");
                var developers = await _userManager.GetUsersInRoleAsync("Developer");

                return new DashboardDto
                {
                    TotalProperties = properties.Count(),
                    AvailableProperties = properties.Count(p => p.Status == PropertyStatus.Available),
                    SoldProperties = properties.Count(p => p.Status == PropertyStatus.Sold),
                    ActiveAgents = agents.Count(a => a.IsActive),
                    InactiveAgents = agents.Count(a => !a.IsActive),
                    ActiveClients = clients.Count(c => c.IsActive),
                    InactiveClients = clients.Count(c => !c.IsActive),
                    ActiveDevelopers = developers.Count(d => d.IsActive),
                    InactiveDevelopers = developers.Count(d => !d.IsActive)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data");
                return new DashboardDto();
            }
        }

        public async Task<List<AgentListDto>> GetAllAgentsAsync()
        {
            try
            {
                var agents = await _userManager.GetUsersInRoleAsync("Agent");
                var result = new List<AgentListDto>();
                foreach (var agent in agents)
                {
                    var properties = await _propertyRepository.GetByAgentIdAsync(agent.Id);
                    result.Add(new AgentListDto
                    {
                        Id = agent.Id,
                        FirstName = agent.FirstName,
                        LastName = agent.LastName,
                        Email = agent.Email,
                        IsActive = agent.IsActive,
                        PropertyCount = properties.Count()
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all agents");
                return [];
            }
        }

        public async Task ToggleAgentStatusAsync(string agentId)
        {
            try
            {
                var agent = await _userManager.FindByIdAsync(agentId);
                if (agent == null) return;

                agent.IsActive = !agent.IsActive;
                await _userManager.UpdateAsync(agent);
                _logger.LogInformation("Agent {AgentId} status changed to {Status}", agentId, agent.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling agent status: {AgentId}", agentId);
            }
        }

        public async Task DeleteAgentAsync(string agentId)
        {
            try
            {
                var agent = await _userManager.FindByIdAsync(agentId);
                if (agent == null) return;

                var properties = await _propertyRepository.GetByAgentIdAsync(agentId);
                foreach (var property in properties)
                {
                    _logger.LogInformation("Deleting property {PropertyId} of agent {AgentId}", property.Id, agentId);
                    await _propertyRepository.DeleteAsync(property.Id);
                }

                await _userManager.DeleteAsync(agent);
                _logger.LogInformation("Agent {AgentId} deleted successfully", agentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agent: {AgentId}", agentId);
            }
        }

        public async Task<List<AdminListDto>> GetAllAdminsAsync()
        {
            try
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                return admins.Select(a => new AdminListDto
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    UserName = a.UserName,
                    Email = a.Email,
                    IdCard = a.IdCard,
                    IsActive = a.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all admins");
                return [];
            }
        }

        public async Task<RegisterAdminDto?> GetAdminByIdAsync(string id)
        {
            try
            {
                var admin = await _userManager.FindByIdAsync(id);
                if (admin == null) return null;
                return new RegisterAdminDto
                {
                    Id = admin.Id,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    IdCard = admin.IdCard,
                    Email = admin.Email,
                    UserName = admin.UserName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin by ID: {Id}", id);
                return null;
            }
        }

        public async Task<string?> CreateAdminAsync(RegisterAdminDto dto)
        {
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email!);
                if (existingEmail != null) return "Ya existe una cuenta con este correo";

                var existingUser = await _userManager.FindByNameAsync(dto.UserName!);
                if (existingUser != null) return "Ya existe una cuenta con este nombre de usuario";

                var user = new ApplicationUser
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    IdCard = dto.IdCard,
                    Email = dto.Email,
                    UserName = dto.UserName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password!);
                if (!result.Succeeded)
                    return string.Join(", ", result.Errors.Select(e => e.Description));

                await _userManager.AddToRoleAsync(user, "Admin");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin");
                return "Ocurrió un error al crear el administrador";
            }
        }

        public async Task<string?> UpdateAdminAsync(string id, RegisterAdminDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return "Administrador no encontrado";

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.IdCard = dto.IdCard;
                user.Email = dto.Email;
                user.UserName = dto.UserName;

                if (!string.IsNullOrEmpty(dto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                    if (!passwordResult.Succeeded)
                        return string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return string.Join(", ", result.Errors.Select(e => e.Description));

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating admin: {Id}", id);
                return "Ocurrió un error al actualizar el administrador";
            }
        }

        public async Task ToggleAdminStatusAsync(string adminId)
        {
            try
            {
                var admin = await _userManager.FindByIdAsync(adminId);
                if (admin == null) return;

                admin.IsActive = !admin.IsActive;
                await _userManager.UpdateAsync(admin);
                _logger.LogInformation("Admin {AdminId} status changed to {Status}", adminId, admin.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling admin status: {AdminId}", adminId);
            }
        }

        public async Task<List<DeveloperListDto>> GetAllDevelopersAsync()
        {
            try
            {
                var developers = await _userManager.GetUsersInRoleAsync("Developer");
                return developers.Select(d => new DeveloperListDto
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    UserName = d.UserName,
                    Email = d.Email,
                    IdCard = d.IdCard,
                    IsActive = d.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all developers");
                return [];
            }
        }

        public async Task<RegisterDeveloperDto?> GetDeveloperByIdAsync(string id)
        {
            try
            {
                var developer = await _userManager.FindByIdAsync(id);
                if (developer == null) return null;
                return new RegisterDeveloperDto
                {
                    Id = developer.Id,
                    FirstName = developer.FirstName,
                    LastName = developer.LastName,
                    IdCard = developer.IdCard,
                    Email = developer.Email,
                    UserName = developer.UserName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting developer by ID: {Id}", id);
                return null;
            }
        }

        public async Task<string?> CreateDeveloperAsync(RegisterDeveloperDto dto)
        {
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email!);
                if (existingEmail != null) return "Ya existe una cuenta con este correo";

                var existingUser = await _userManager.FindByNameAsync(dto.UserName!);
                if (existingUser != null) return "Ya existe una cuenta con este nombre de usuario";

                var user = new ApplicationUser
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    IdCard = dto.IdCard,
                    Email = dto.Email,
                    UserName = dto.UserName,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password!);
                if (!result.Succeeded)
                    return string.Join(", ", result.Errors.Select(e => e.Description));

                await _userManager.AddToRoleAsync(user, "Developer");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating developer");
                return "Ocurrió un error al crear el desarrollador";
            }
        }

        public async Task<string?> UpdateDeveloperAsync(string id, RegisterDeveloperDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return "Desarrollador no encontrado";

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.IdCard = dto.IdCard;
                user.Email = dto.Email;
                user.UserName = dto.UserName;

                if (!string.IsNullOrEmpty(dto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                    if (!passwordResult.Succeeded)
                        return string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return string.Join(", ", result.Errors.Select(e => e.Description));

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating developer: {Id}", id);
                return "Ocurrió un error al actualizar el desarrollador";
            }
        }

        public async Task ToggleDeveloperStatusAsync(string developerId)
        {
            try
            {
                var developer = await _userManager.FindByIdAsync(developerId);
                if (developer == null) return;

                developer.IsActive = !developer.IsActive;
                await _userManager.UpdateAsync(developer);
                _logger.LogInformation("Developer {DeveloperId} status changed to {Status}", developerId, developer.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling developer status: {DeveloperId}", developerId);
            }
        }

        public async Task<List<ClientListDto>> GetAllClientsAsync()
        {
            try
            {
                var clients = await _userManager.GetUsersInRoleAsync("Client");
                return clients.Select(c => new ClientListDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    UserName = c.UserName,
                    Email = c.Email,
                    IsActive = c.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all clients");
                return [];
            }
        }

        public async Task ToggleClientStatusAsync(string clientId)
        {
            try
            {
                var client = await _userManager.FindByIdAsync(clientId);
                if (client == null) return;

                // Logica de negocio: cambiar estado del cliente
                client.IsActive = !client.IsActive;
                await _userManager.UpdateAsync(client);
                _logger.LogInformation("Client {ClientId} status changed to {Status}", clientId, client.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling client status: {ClientId}", clientId);
            }
        }
    }
}