using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.Dtos.Agent;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Domain.Interfaces;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Services
{
    public class AgentService : IAgentService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ILogger<AgentService> _logger;

        public AgentService(UserManager<ApplicationUser> userManager,
            IPropertyRepository propertyRepository,
            ILogger<AgentService> logger)
        {
            _userManager = userManager;
            _propertyRepository = propertyRepository;
            _logger = logger;
        }

        public async Task<List<AgentDto>> GetAllAsync()
        {
            try
            {
                var agents = await _userManager.GetUsersInRoleAsync("Agent");
                var activeAgents = agents.Where(a => a.IsActive).OrderBy(a => a.FirstName).ToList();

                var result = new List<AgentDto>();
                foreach (var agent in activeAgents)
                {
                    var properties = await _propertyRepository.GetByAgentIdAsync(agent.Id);
                    result.Add(new AgentDto
                    {
                        Id = agent.Id,
                        FirstName = agent.FirstName,
                        LastName = agent.LastName,
                        Email = agent.Email,
                        Phone = agent.Phone,
                        PhotoUrl = agent.PhotoUrl,
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

        public async Task<AgentDto?> GetByIdAsync(string id)
        {
            try
            {
                var agent = await _userManager.FindByIdAsync(id);
                if (agent == null) return null;

                var properties = await _propertyRepository.GetByAgentIdAsync(agent.Id);
                return new AgentDto
                {
                    Id = agent.Id,
                    FirstName = agent.FirstName,
                    LastName = agent.LastName,
                    Email = agent.Email,
                    Phone = agent.Phone,
                    PhotoUrl = agent.PhotoUrl,
                    IsActive = agent.IsActive,
                    PropertyCount = properties.Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agent by ID: {Id}", id);
                return null;
            }
        }

        public async Task<List<AgentDto>> SearchByNameAsync(string name)
        {
            try
            {
                var agents = await _userManager.GetUsersInRoleAsync("Agent");
                var filtered = agents
                    .Where(a => a.IsActive &&
                        ($"{a.FirstName} {a.LastName}").Contains(name, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(a => a.FirstName)
                    .ToList();

                var result = new List<AgentDto>();
                foreach (var agent in filtered)
                {
                    var properties = await _propertyRepository.GetByAgentIdAsync(agent.Id);
                    result.Add(new AgentDto
                    {
                        Id = agent.Id,
                        FirstName = agent.FirstName,
                        LastName = agent.LastName,
                        Email = agent.Email,
                        Phone = agent.Phone,
                        PhotoUrl = agent.PhotoUrl,
                        IsActive = agent.IsActive,
                        PropertyCount = properties.Count()
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching agents by name: {Name}", name);
                return [];
            }
        }

        public async Task<string?> UpdateProfileAsync(string agentId, string firstName,
            string lastName, string phone, string? photoUrl)
        {
            try
            {
                var agent = await _userManager.FindByIdAsync(agentId);
                if (agent == null) return "Agente no encontrado";

                agent.FirstName = firstName;
                agent.LastName = lastName;
                agent.Phone = phone;

                if (!string.IsNullOrEmpty(photoUrl))
                    agent.PhotoUrl = photoUrl;

                var result = await _userManager.UpdateAsync(agent);
                if (!result.Succeeded)
                    return string.Join(", ", result.Errors.Select(e => e.Description));

                _logger.LogInformation("Agent {AgentId} profile updated successfully", agentId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent profile: {AgentId}", agentId);
                return "Ocurrió un error al actualizar el perfil";
            }
        }

        public async Task<string?> GetClientNameAsync(string clientId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(clientId);
                if (user == null) return null;
                return $"{user.FirstName} {user.LastName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client name: {ClientId}", clientId);
                return null;
            }
        }

        public async Task<List<PropertyDto>> GetAgentPropertiesAsync(string agentId)
        {
            try
            {
                var properties = await _propertyRepository.GetByAgentIdAsync(agentId);
                return properties.Select(p => new PropertyDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Price = p.Price,
                    LandSize = p.LandSize,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    Description = p.Description,
                    AgentId = p.AgentId,
                    Status = p.Status.ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agent properties: {AgentId}", agentId);
                return [];
            }
        }

        public async Task ChangeStatusAsync(string agentId, bool isActive)
        {
            try
            {
                var agent = await _userManager.FindByIdAsync(agentId);
                if (agent == null) return;

                agent.IsActive = isActive;
                await _userManager.UpdateAsync(agent);
                _logger.LogInformation("Agent {AgentId} status changed to {Status}", agentId, isActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing agent status: {AgentId}", agentId);
            }
        }
    }
}