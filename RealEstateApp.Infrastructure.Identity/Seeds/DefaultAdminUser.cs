using Microsoft.AspNetCore.Identity;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Seeds
{
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                FirstName = "Admin",
                LastName = "Default",
                UserName = "admindefault",
                Email = "admin@realestate.com",
                IsActive = true,
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(user.Email!) == null)
            {
                await userManager.CreateAsync(user, "Admin123!");
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}