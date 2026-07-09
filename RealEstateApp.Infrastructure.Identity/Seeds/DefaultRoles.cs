using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.Admin.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));

            if (!await roleManager.RoleExistsAsync(Roles.Agent.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Agent.ToString()));

            if (!await roleManager.RoleExistsAsync(Roles.Client.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Client.ToString()));

            if (!await roleManager.RoleExistsAsync(Roles.Developer.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Roles.Developer.ToString()));
        }
    }
}