using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Dtos.Email;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Domain.Settings;
using RealEstateApp.Infrastructure.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RealEstateApp.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountService> _logger;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;

        public AccountService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountService> logger,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            AuthenticationResponse response = new();
            try
            {
                var user = await _userManager.FindByEmailAsync(request.UsernameOrEmail!)
                           ?? await _userManager.FindByNameAsync(request.UsernameOrEmail!);

                if (user == null)
                {
                    response.HasError = true;
                    response.Error = "No existe una cuenta con este usuario o correo";
                    return response;
                }

                if (!user.IsActive)
                {
                    response.HasError = true;
                    response.Error = "Tu cuenta no está activa, contacta al administrador";
                    return response;
                }

                if (!user.EmailConfirmed)
                {
                    response.HasError = true;
                    response.Error = "Tu cuenta no está confirmada, revisa tu correo";
                    return response;
                }

                var result = await _signInManager.PasswordSignInAsync(user, request.Password!, false, false);

                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Error = "Contraseña incorrecta";
                    return response;
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Developer"))
                {
                    await _signInManager.SignOutAsync();
                    response.HasError = true;
                    response.Error = "Los desarrolladores no tienen acceso al WebApp";
                    return response;
                }

                response.Id = user.Id;
                response.FirstName = user.FirstName;
                response.LastName = user.LastName;
                response.UserName = user.UserName;
                response.Email = user.Email;
                response.Role = roles.FirstOrDefault();
                response.IsActive = user.IsActive;

                _logger.LogInformation("User {UserName} logged in successfully", user.UserName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user");
                response.HasError = true;
                response.Error = "Ocurrió un error al iniciar sesión";
                return response;
            }
        }

        public async Task<AuthenticationResponse> AuthenticateForApiAsync(AuthenticationRequest request)
        {
            AuthenticationResponse response = new();
            try
            {
                var user = await _userManager.FindByEmailAsync(request.UsernameOrEmail!)
                           ?? await _userManager.FindByNameAsync(request.UsernameOrEmail!);

                if (user == null)
                {
                    response.HasError = true;
                    response.Error = "No existe una cuenta con este usuario o correo";
                    return response;
                }

                if (!user.IsActive)
                {
                    response.HasError = true;
                    response.Error = "Tu cuenta no está activa";
                    return response;
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, false);

                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Error = "Contraseña incorrecta";
                    return response;
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Contains("Admin") && !roles.Contains("Developer"))
                {
                    response.HasError = true;
                    response.Error = "No tienes permisos para acceder a la API";
                    return response;
                }

                response.Id = user.Id;
                response.FirstName = user.FirstName;
                response.LastName = user.LastName;
                response.UserName = user.UserName;
                response.Email = user.Email;
                response.Role = roles.FirstOrDefault();
                response.IsActive = user.IsActive;
                response.JwtToken = GenerateJwtToken(user, roles.ToList());

                _logger.LogInformation("User {UserName} logged in to API successfully", user.UserName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user for API");
                response.HasError = true;
                response.Error = "Ocurrió un error al iniciar sesión";
                return response;
            }
        }

        public async Task<RegisterResponse> RegisterDeveloperAsync(RegisterRequest request)
        {
            RegisterResponse response = new();
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.Email!);
                if (existingEmail != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este correo";
                    return response;
                }

                var existingUser = await _userManager.FindByNameAsync(request.UserName!);
                if (existingUser != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este nombre de usuario";
                    return response;
                }

                var user = new ApplicationUser
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.UserName,
                    Email = request.Email,
                    Phone = request.Phone,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password!);
                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Error = string.Join(", ", result.Errors.Select(e => e.Description));
                    return response;
                }

                await _userManager.AddToRoleAsync(user, "Developer");
                _logger.LogInformation("Developer {UserName} registered successfully", user.UserName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering developer");
                response.HasError = true;
                response.Error = "Ocurrió un error al registrar el desarrollador";
                return response;
            }
        }

        public async Task<RegisterResponse> RegisterAdminAsync(RegisterRequest request)
        {
            RegisterResponse response = new();
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.Email!);
                if (existingEmail != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este correo";
                    return response;
                }

                var existingUser = await _userManager.FindByNameAsync(request.UserName!);
                if (existingUser != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este nombre de usuario";
                    return response;
                }

                var user = new ApplicationUser
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.UserName,
                    Email = request.Email,
                    Phone = request.Phone,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password!);
                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Error = string.Join(", ", result.Errors.Select(e => e.Description));
                    return response;
                }

                await _userManager.AddToRoleAsync(user, "Admin");
                _logger.LogInformation("Admin {UserName} registered successfully", user.UserName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering admin");
                response.HasError = true;
                response.Error = "Ocurrió un error al registrar el administrador";
                return response;
            }
        }

        public async Task<RegisterResponse> RegisterClientAsync(RegisterRequest request)
        {
            RegisterResponse response = new();
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.Email!);
                if (existingEmail != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este correo";
                    return response;
                }

                var existingUser = await _userManager.FindByNameAsync(request.UserName!);
                if (existingUser != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este nombre de usuario";
                    return response;
                }

                var user = new ApplicationUser
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.UserName,
                    Email = request.Email,
                    Phone = request.Phone,
                    PhotoUrl = request.PhotoUrl,
                    IsActive = false,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, request.Password!);
                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Error = string.Join(", ", result.Errors.Select(e => e.Description));
                    return response;
                }

                await _userManager.AddToRoleAsync(user, "Client");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"https://localhost:7120/Login/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Confirma tu cuenta - RealEstateApp",
                    HtmlBody = $@"
                        <h2>Bienvenido a RealEstateApp</h2>
                        <p>Hola {user.FirstName} {user.LastName},</p>
                        <p>Gracias por registrarte. Por favor confirma tu cuenta haciendo click en el siguiente enlace:</p>
                        <a href='{confirmationLink}' style='background-color:#28a745;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>
                            Confirmar mi cuenta
                        </a>
                        <p>Si no te registraste en RealEstateApp, ignora este correo.</p>"
                });

                _logger.LogInformation("Client {UserName} registered successfully", user.UserName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering client");
                response.HasError = true;
                response.Error = "Ocurrió un error al registrar el cliente";
                return response;
            }
        }

        public async Task<RegisterResponse> RegisterAgentAsync(RegisterRequest request)
        {
            RegisterResponse response = new();
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.Email!);
                if (existingEmail != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este correo";
                    return response;
                }

                var existingUser = await _userManager.FindByNameAsync(request.UserName!);
                if (existingUser != null)
                {
                    response.HasError = true;
                    response.Error = "Ya existe una cuenta con este nombre de usuario";
                    return response;
                }

                var user = new ApplicationUser
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.UserName,
                    Email = request.Email,
                    Phone = request.Phone,
                    PhotoUrl = request.PhotoUrl,
                    IsActive = false,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password!);
                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Error = string.Join(", ", result.Errors.Select(e => e.Description));
                    return response;
                }

                await _userManager.AddToRoleAsync(user, "Agent");
                _logger.LogInformation("Agent {UserName} registered successfully", user.UserName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent");
                response.HasError = true;
                response.Error = "Ocurrió un error al registrar el agente";
                return response;
            }
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User signed out successfully");
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    user.IsActive = true;
                    await _userManager.UpdateAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
                return false;
            }
        }

        #region Private methods
        private string GenerateJwtToken(ApplicationUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!),
                new("uid", user.Id)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}