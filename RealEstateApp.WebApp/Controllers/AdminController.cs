using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Admin;
using RealEstateApp.Core.Application.Interfaces;
using System.Security.Claims;

namespace RealEstateApp.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;

        public AdminController(IAdminService adminService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService)
        {
            _adminService = adminService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var dashboard = await _adminService.GetDashboardAsync();
            return View(dashboard);
        }

        #region Agentes
        public async Task<IActionResult> Agents()
        {
            var agents = await _adminService.GetAllAgentsAsync();
            return View(agents);
        }

        public async Task<IActionResult> ToggleAgentStatus(string id)
        {
            await _adminService.ToggleAgentStatusAsync(id);
            return RedirectToAction("Agents");
        }

        public async Task<IActionResult> DeleteAgent(string id)
        {
            var agent = await _adminService.GetAllAgentsAsync();
            var found = agent.FirstOrDefault(a => a.Id == id);
            if (found == null) return NotFound();
            return View(found);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAgentConfirmed(string id)
        {
            await _adminService.DeleteAgentAsync(id);
            return RedirectToAction("Agents");
        }
        #endregion

        #region Administradores
        public async Task<IActionResult> Admins()
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return View(admins);
        }

        public IActionResult CreateAdmin()
        {
            return View(new RegisterAdminDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(RegisterAdminDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var error = await _adminService.CreateAdminAsync(dto);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(dto);
            }
            return RedirectToAction("Admins");
        }

        public async Task<IActionResult> EditAdmin(string id)
        {
            // Logica de negocio: no puede editar su propio usuario
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == currentUserId)
            {
                TempData["Error"] = "No puedes editar tu propio usuario";
                return RedirectToAction("Admins");
            }

            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        [HttpPost]
        public async Task<IActionResult> EditAdmin(string id, RegisterAdminDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var error = await _adminService.UpdateAdminAsync(id, dto);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(dto);
            }
            return RedirectToAction("Admins");
        }

        public async Task<IActionResult> ToggleAdminStatus(string id)
        {
            // Logica de negocio: no puede inactivar su propio usuario
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == currentUserId)
            {
                TempData["Error"] = "No puedes activar/inactivar tu propio usuario";
                return RedirectToAction("Admins");
            }

            await _adminService.ToggleAdminStatusAsync(id);
            return RedirectToAction("Admins");
        }
        #endregion

        #region Desarrolladores
        public async Task<IActionResult> Developers()
        {
            var developers = await _adminService.GetAllDevelopersAsync();
            return View(developers);
        }

        public IActionResult CreateDeveloper()
        {
            return View(new RegisterDeveloperDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeveloper(RegisterDeveloperDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var error = await _adminService.CreateDeveloperAsync(dto);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(dto);
            }
            return RedirectToAction("Developers");
        }

        public async Task<IActionResult> EditDeveloper(string id)
        {
            var developer = await _adminService.GetDeveloperByIdAsync(id);
            if (developer == null) return NotFound();
            return View(developer);
        }

        [HttpPost]
        public async Task<IActionResult> EditDeveloper(string id, RegisterDeveloperDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var error = await _adminService.UpdateDeveloperAsync(id, dto);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(dto);
            }
            return RedirectToAction("Developers");
        }

        public async Task<IActionResult> ToggleDeveloperStatus(string id)
        {
            await _adminService.ToggleDeveloperStatusAsync(id);
            return RedirectToAction("Developers");
        }
        #endregion

        #region Tipo de propiedades
        public async Task<IActionResult> PropertyTypes()
        {
            var types = await _propertyTypeService.GetAll();
            return View(types);
        }

        public IActionResult CreatePropertyType()
        {
            return View(new Core.Application.Dtos.PropertyType.PropertyTypeDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreatePropertyType(Core.Application.Dtos.PropertyType.PropertyTypeDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _propertyTypeService.AddAsync(dto);
            return RedirectToAction("PropertyTypes");
        }

        public async Task<IActionResult> EditPropertyType(int id)
        {
            var type = await _propertyTypeService.GetById(id);
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        public async Task<IActionResult> EditPropertyType(int id, Core.Application.Dtos.PropertyType.PropertyTypeDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _propertyTypeService.UpdateAsync(dto, id);
            return RedirectToAction("PropertyTypes");
        }

        public async Task<IActionResult> DeletePropertyType(int id)
        {
            var type = await _propertyTypeService.GetById(id);
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePropertyTypeConfirmed(int id)
        {
            await _propertyTypeService.DeleteAsync(id);
            return RedirectToAction("PropertyTypes");
        }
        #endregion

        #region Tipo de ventas
        public async Task<IActionResult> SaleTypes()
        {
            var types = await _saleTypeService.GetAll();
            return View(types);
        }

        public IActionResult CreateSaleType()
        {
            return View(new Core.Application.Dtos.SaleType.SaleTypeDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateSaleType(Core.Application.Dtos.SaleType.SaleTypeDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _saleTypeService.AddAsync(dto);
            return RedirectToAction("SaleTypes");
        }

        public async Task<IActionResult> EditSaleType(int id)
        {
            var type = await _saleTypeService.GetById(id);
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        public async Task<IActionResult> EditSaleType(int id, Core.Application.Dtos.SaleType.SaleTypeDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _saleTypeService.UpdateAsync(dto, id);
            return RedirectToAction("SaleTypes");
        }

        public async Task<IActionResult> DeleteSaleType(int id)
        {
            var type = await _saleTypeService.GetById(id);
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSaleTypeConfirmed(int id)
        {
            await _saleTypeService.DeleteAsync(id);
            return RedirectToAction("SaleTypes");
        }
        #endregion

        #region Mejoras
        public async Task<IActionResult> Improvements()
        {
            var improvements = await _improvementService.GetAll();
            return View(improvements);
        }

        public IActionResult CreateImprovement()
        {
            return View(new Core.Application.Dtos.Improvement.ImprovementDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateImprovement(Core.Application.Dtos.Improvement.ImprovementDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _improvementService.AddAsync(dto);
            return RedirectToAction("Improvements");
        }

        public async Task<IActionResult> EditImprovement(int id)
        {
            var improvement = await _improvementService.GetById(id);
            if (improvement == null) return NotFound();
            return View(improvement);
        }

        [HttpPost]
        public async Task<IActionResult> EditImprovement(int id, Core.Application.Dtos.Improvement.ImprovementDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _improvementService.UpdateAsync(dto, id);
            return RedirectToAction("Improvements");
        }

        public async Task<IActionResult> DeleteImprovement(int id)
        {
            var improvement = await _improvementService.GetById(id);
            if (improvement == null) return NotFound();
            return View(improvement);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImprovementConfirmed(int id)
        {
            await _improvementService.DeleteAsync(id);
            return RedirectToAction("Improvements");
        }

        #endregion

        #region Clientes
        public async Task<IActionResult> Clients()
        {
            var clients = await _adminService.GetAllClientsAsync();
            return View(clients);
        }

        public async Task<IActionResult> ToggleClientStatus(string id)
        {
            await _adminService.ToggleClientStatusAsync(id);
            return RedirectToAction("Clients");
        }
        #endregion
    }
}