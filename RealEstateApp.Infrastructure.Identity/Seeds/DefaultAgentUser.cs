using Microsoft.AspNetCore.Identity;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Seeds
{
    public static class DefaultAgentUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                FirstName = "Agent",
                LastName = "Default",
                UserName = "agentdefault",
                Email = "agent@realestate.com",
                IsActive = true,
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(user.Email!) == null)
            {
                await userManager.CreateAsync(user, "Agent123!");
                await userManager.AddToRoleAsync(user, "Agent");
            }
        }
    }
}