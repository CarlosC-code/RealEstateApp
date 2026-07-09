using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;

        public LoginController(IAccountService accountService, IEmailService emailService)
        {
            _accountService = accountService;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToHome();
            return View(new AuthenticationRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Index(AuthenticationRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);
            var response = await _accountService.AuthenticateAsync(request);
            if (response.HasError)
            {
                ModelState.AddModelError("", response.Error!);
                return View(request);
            }
            return response.Role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Agent" => RedirectToAction("Index", "Agent"),
                "Client" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToHome();
            return View(new RegisterRequest());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request, string userType)
        {
            if (!ModelState.IsValid)
                return View(request);
            RegisterResponse response;
            if (userType == "Client")
                response = await _accountService.RegisterClientAsync(request);
            else
                response = await _accountService.RegisterAgentAsync(request);
            if (response.HasError)
            {
                ModelState.AddModelError("", response.Error!);
                return View(request);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> LogOut()
        {
            await _accountService.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Index");
            var result = await _accountService.ConfirmEmailAsync(userId, token);
            if (result)
                TempData["Success"] = "Tu cuenta ha sido confirmada. Ya puedes iniciar sesión.";
            else
                TempData["Error"] = "Error al confirmar tu cuenta. El enlace puede haber expirado.";
            return RedirectToAction("Index");
        }

        private IActionResult RedirectToHome()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Agent"))
                return RedirectToAction("Index", "Agent");
            return RedirectToAction("Index", "Home");
        }
    }
}