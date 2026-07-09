using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using System.Security.Claims;

namespace RealEstateApp.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly IFavoritePropertyService _favoritePropertyService;
        private readonly IAgentService _agentService;
        private readonly IChatMessageService _chatMessageService;
        private readonly IOfferService _offerService;

        public HomeController(IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            IFavoritePropertyService favoritePropertyService,
            IAgentService agentService,
            IChatMessageService chatMessageService,
            IOfferService offerService)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _favoritePropertyService = favoritePropertyService;
            _agentService = agentService;
            _chatMessageService = chatMessageService;
            _offerService = offerService;
        }

        public async Task<IActionResult> Index(int? propertyTypeId, decimal? minPrice,
            decimal? maxPrice, int? bedrooms, int? bathrooms, string? code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                var property = await _propertyService.GetByCodeAsync(code);
                var singleList = property != null
                    ? new List<Core.Application.Dtos.Property.PropertyDto> { property }
                    : new List<Core.Application.Dtos.Property.PropertyDto>();
                ViewBag.PropertyTypes = await _propertyTypeService.GetAll();
                ViewBag.ClientId = User.Identity!.IsAuthenticated
                    ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    : null;
                return View(singleList);
            }

            var properties = await _propertyService.GetAllWithFiltersAsync(
                propertyTypeId, minPrice, maxPrice, bedrooms, bathrooms);
            ViewBag.PropertyTypes = await _propertyTypeService.GetAll();
            ViewBag.ClientId = User.Identity!.IsAuthenticated
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                : null;
            return View(properties);
        }

        public async Task<IActionResult> MyProperties()
        {
            var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (clientId == null) return RedirectToAction("Index", "Login");
            var favorites = await _favoritePropertyService.GetByClientIdAsync(clientId);
            return View(favorites);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var property = await _propertyService.GetById(id);
            if (property == null) return NotFound();

            if (!string.IsNullOrEmpty(property.AgentId))
            {
                var agent = await _agentService.GetByIdAsync(property.AgentId);
                ViewBag.Agent = agent;

                var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (clientId != null && User.IsInRole("Client"))
                {
                    var messages = await _chatMessageService.GetByPropertyAndUsersAsync(
                        id, clientId, property.AgentId);
                    ViewBag.ChatMessages = messages;
                    ViewBag.ClientId = clientId;
                    ViewBag.AgentId = property.AgentId;

                    var offers = await _offerService.GetByPropertyIdAsync(id);
                    var clientOffers = offers.Where(o => o.ClientId == clientId).ToList();
                    ViewBag.ClientOffers = clientOffers;
                    ViewBag.HasAcceptedOffer = offers.Any(o => o.Status == "Accepted");
                }

                if (User.IsInRole("Agent"))
                {
                    var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var allMessages = await _chatMessageService.GetConversationsByAgentAsync(agentId!);
                    var propertyMessages = allMessages.Where(m => m.PropertyId == id).ToList();
                    var clientIds = propertyMessages
                        .Select(m => m.IsFromClient ? m.SenderId : m.ReceiverId)
                        .Distinct().ToList();

                    var clients = new List<object>();
                    foreach (var cId in clientIds)
                    {
                        var clientName = await _agentService.GetClientNameAsync(cId!) ?? cId;
                        clients.Add(new { ClientId = cId, ClientName = clientName });
                    }
                    ViewBag.ChatClients = clients;
                    ViewBag.PropertyId = id;
                }
            }

            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int propertyId, string agentId, string message)
        {
            var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (clientId == null) return RedirectToAction("Index", "Login");

            await _chatMessageService.SendMessageAsync(propertyId, clientId, agentId, message);
            return RedirectToAction("Detail", new { id = propertyId });
        }

        [HttpPost]
        public async Task<IActionResult> MakeOffer(int propertyId, decimal amount)
        {
            var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (clientId == null) return RedirectToAction("Index", "Login");

            await _offerService.AddAsync(new Core.Application.Dtos.Offer.OfferDto
            {
                PropertyId = propertyId,
                ClientId = clientId,
                Amount = amount
            });

            return RedirectToAction("Detail", new { id = propertyId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int propertyId)
        {
            var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (clientId == null) return RedirectToAction("Index", "Login");

            var favorites = await _favoritePropertyService.GetByClientIdAsync(clientId);
            var existing = favorites.FirstOrDefault(f => f.PropertyId == propertyId);

            if (existing != null)
                await _favoritePropertyService.DeleteAsync(existing.Id);
            else
                await _favoritePropertyService.AddAsync(new Core.Application.Dtos.FavoriteProperty.FavoritePropertyDto
                {
                    PropertyId = propertyId,
                    ClientId = clientId
                });

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MyOffers()
        {
            var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (clientId == null) return RedirectToAction("Index", "Login");

            var offers = await _offerService.GetByClientIdAsync(clientId);
            return View(offers);
        }
    }
}