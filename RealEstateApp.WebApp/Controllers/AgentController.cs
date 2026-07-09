using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.Interfaces;
using System.Security.Claims;

namespace RealEstateApp.WebApp.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;
        private readonly IAgentService _agentService;
        private readonly IChatMessageService _chatMessageService;
        private readonly IOfferService _offerService;

        public AgentController(IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IAgentService agentService,
            IChatMessageService chatMessageService,
            IOfferService offerService)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _agentService = agentService;
            _chatMessageService = chatMessageService;
            _offerService = offerService;
        }

        public async Task<IActionResult> Index()
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var properties = await _propertyService.GetByAgentIdAsync(agentId!);
            return View(properties);
        }

        public async Task<IActionResult> MyProfile()
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var agent = await _agentService.GetByIdAsync(agentId!);
            return View(agent);
        }

        [HttpPost]
        public async Task<IActionResult> MyProfile(string firstName, string lastName,
            string phone, IFormFile? photo)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? photoUrl = null;

            if (photo != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "agents", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using var stream = new FileStream(filePath, FileMode.Create);
                await photo.CopyToAsync(stream);
                photoUrl = $"/images/agents/{fileName}";
            }

            var error = await _agentService.UpdateProfileAsync(agentId!, firstName, lastName, phone, photoUrl);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("MyProfile");
            }

            TempData["Success"] = "Perfil actualizado correctamente";
            return RedirectToAction("MyProfile");
        }

        public async Task<IActionResult> Chats()
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = await _chatMessageService.GetConversationsByAgentAsync(agentId!);

            var conversations = new List<object>();
            foreach (var group in messages
                .GroupBy(m => new { m.PropertyId, ClientId = m.IsFromClient ? m.SenderId : m.ReceiverId }))
            {
                var clientName = await _agentService.GetClientNameAsync(group.Key.ClientId!) ?? group.Key.ClientId;
                var property = await _propertyService.GetById(group.Key.PropertyId);
                conversations.Add(new
                {
                    PropertyId = group.Key.PropertyId,
                    PropertyCode = property?.Code ?? group.Key.PropertyId.ToString(),
                    PropertyType = property?.PropertyTypeName ?? "",
                    ClientId = group.Key.ClientId,
                    ClientName = clientName,
                    Messages = group.OrderBy(m => m.SentAt).ToList()
                });
            }

            ViewBag.Conversations = conversations;
            ViewBag.AgentId = agentId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReplyChat(int propertyId, string clientId, string message)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _chatMessageService.SendMessageAsync(propertyId, agentId!, clientId, message);
            return RedirectToAction("Chats");
        }

        public async Task<IActionResult> ChatWithClient(int propertyId, string clientId)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var property = await _propertyService.GetById(propertyId);
            var clientName = await _agentService.GetClientNameAsync(clientId) ?? clientId;
            var messages = await _chatMessageService.GetByPropertyAndUsersAsync(
                propertyId, agentId!, clientId);

            ViewBag.Property = property;
            ViewBag.ClientId = clientId;
            ViewBag.ClientName = clientName;
            ViewBag.AgentId = agentId;
            ViewBag.Messages = messages;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageToClient(int propertyId, string clientId, string message)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _chatMessageService.SendMessageAsync(propertyId, agentId!, clientId, message);
            return RedirectToAction("ChatWithClient", new { propertyId, clientId });
        }

        public async Task<IActionResult> Offers(int propertyId)
        {
            var property = await _propertyService.GetById(propertyId);
            if (property == null) return NotFound();

            var offers = await _offerService.GetByPropertyIdAsync(propertyId);
            var offerList = new List<object>();

            foreach (var offer in offers)
            {
                var clientName = await _agentService.GetClientNameAsync(offer.ClientId!) ?? offer.ClientId;
                offerList.Add(new
                {
                    offer.Id,
                    offer.PropertyId,
                    offer.ClientId,
                    ClientName = clientName,
                    offer.Amount,
                    offer.OfferDate,
                    offer.Status
                });
            }

            ViewBag.Property = property;
            ViewBag.Offers = offerList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOffer(int offerId, int propertyId)
        {
            await _offerService.AcceptAsync(offerId);
            return RedirectToAction("Offers", new { propertyId });
        }

        [HttpPost]
        public async Task<IActionResult> RejectOffer(int offerId, int propertyId)
        {
            await _offerService.RejectAsync(offerId);
            return RedirectToAction("Offers", new { propertyId });
        }

        #region Mantenimiento de propiedades
        public async Task<IActionResult> Properties()
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var properties = await _propertyService.GetByAgentIdAsync(agentId!);
            return View(properties);
        }

        public async Task<IActionResult> CreateProperty()
        {
            var propertyTypes = await _propertyTypeService.GetAll();
            var saleTypes = await _saleTypeService.GetAll();
            var improvements = await _improvementService.GetAll();

            if (!propertyTypes.Any() || !saleTypes.Any() || !improvements.Any())
            {
                TempData["Error"] = "No hay tipos de propiedades, tipos de ventas o mejoras creadas.";
                return RedirectToAction("Properties");
            }

            ViewBag.PropertyTypes = propertyTypes;
            ViewBag.SaleTypes = saleTypes;
            ViewBag.Improvements = improvements;
            return View(new PropertyDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProperty(PropertyDto dto, List<IFormFile> images)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            dto.AgentId = agentId;

            if (!ModelState.IsValid)
            {
                ViewBag.PropertyTypes = await _propertyTypeService.GetAll();
                ViewBag.SaleTypes = await _saleTypeService.GetAll();
                ViewBag.Improvements = await _improvementService.GetAll();
                return View(dto);
            }

            if (images != null && images.Any())
            {
                dto.Images = new List<string>();
                foreach (var image in images.Take(4))
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "properties", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);
                    dto.Images.Add($"/images/properties/{fileName}");
                }
            }

            await _propertyService.AddAsync(dto);
            return RedirectToAction("Properties");
        }

        public async Task<IActionResult> EditProperty(int id)
        {
            var property = await _propertyService.GetById(id);
            if (property == null) return NotFound();

            ViewBag.PropertyTypes = await _propertyTypeService.GetAll();
            ViewBag.SaleTypes = await _saleTypeService.GetAll();
            ViewBag.Improvements = await _improvementService.GetAll();
            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> EditProperty(int id, PropertyDto dto, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PropertyTypes = await _propertyTypeService.GetAll();
                ViewBag.SaleTypes = await _saleTypeService.GetAll();
                ViewBag.Improvements = await _improvementService.GetAll();
                return View(dto);
            }

            dto.Images = new List<string>();

            if (images != null && images.Any())
            {
                foreach (var image in images.Take(4))
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "properties", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);
                    dto.Images.Add($"/images/properties/{fileName}");
                }
            }

            await _propertyService.UpdateAsync(dto, id);
            return RedirectToAction("Properties");
        }

        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _propertyService.GetById(id);
            if (property == null) return NotFound();
            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePropertyConfirmed(int id)
        {
            await _propertyService.DeleteAsync(id);
            return RedirectToAction("Properties");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int propertyId, string? imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _propertyService.DeleteImageAsync(propertyId, imageUrl);
            }
            return RedirectToAction("EditProperty", new { id = propertyId });
        }
        #endregion
    }
}