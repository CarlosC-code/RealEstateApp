using RealEstateApp.Core.Application.Dtos.Account;

namespace RealEstateApp.Core.Application.Interfaces
{
    public interface IAccountService
    {
        // WebApp
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<RegisterResponse> RegisterClientAsync(RegisterRequest request);
        Task<RegisterResponse> RegisterAgentAsync(RegisterRequest request);
        Task SignOutAsync();
        Task<bool> ConfirmEmailAsync(string userId, string token);

        // WebApi
        Task<AuthenticationResponse> AuthenticateForApiAsync(AuthenticationRequest request);
        Task<RegisterResponse> RegisterDeveloperAsync(RegisterRequest request);
        Task<RegisterResponse> RegisterAdminAsync(RegisterRequest request);
    }
}