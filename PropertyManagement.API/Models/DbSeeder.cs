using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PropertyManagement.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // =====================
            // 1. Seed Roles
            // =====================
            string[] roles = { "PropertyManager", "MaintenanceStaff", "Tenant" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // =====================
            // 2. Seed Users
            // =====================

            // Property Manager
            await CreateUserAsync(
                userManager,
                email: "manager@property.com",
                password: "Manager@123",
                role: "PropertyManager"
            );

            // Maintenance Staff
            await CreateUserAsync(
                userManager,
                email: "staff@property.com",
                password: "Staff@123",
                role: "MaintenanceStaff"
            );

            // Tenant 1
            await CreateUserAsync(
                userManager,
                email: "tenant1@example.com",
                password: "Tenant@123",
                role: "Tenant"
            );

            // Tenant 2
            await CreateUserAsync(
                userManager,
                email: "tenant2@example.com",
                password: "Tenant@123",
                role: "Tenant"
            );
        }

        private static async Task CreateUserAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            // Check if user already exists
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}