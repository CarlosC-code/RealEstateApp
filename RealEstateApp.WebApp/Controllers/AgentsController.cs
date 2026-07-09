using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.WebApp.Controllers
{
    public class AgentsController : Controller
    {
        private readonly IAgentService _agentService;
        private readonly IPropertyService _propertyService;

        public AgentsController(IAgentService agentService, IPropertyService propertyService)
        {
            _agentService = agentService;
            _propertyService = propertyService;
        }

        public async Task<IActionResult> Index(string? name)
        {
            var agents = string.IsNullOrEmpty(name)
                ? await _agentService.GetAllAsync()
                : await _agentService.SearchByNameAsync(name);

            ViewBag.SearchName = name;
            return View(agents);
        }

        public async Task<IActionResult> Properties(string id)
        {
            var agent = await _agentService.GetByIdAsync(id);
            if (agent == null) return NotFound();

            var properties = await _propertyService.GetByAgentIdAsync(id);
            ViewBag.Agent = agent;
            return View(properties);
        }
    }
}