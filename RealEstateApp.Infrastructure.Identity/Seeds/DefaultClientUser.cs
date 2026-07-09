using Microsoft.AspNetCore.Identity;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Seeds
{
    public static class DefaultClientUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                FirstName = "Client",
                LastName = "Default",
                UserName = "clientdefault",
                Email = "client@realestate.com",
                IsActive = true,
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(user.Email!) == null)
            {
                await userManager.CreateAsync(user, "Client123!");
                await userManager.AddToRoleAsync(user, "Client");
            }
        }
    }
}