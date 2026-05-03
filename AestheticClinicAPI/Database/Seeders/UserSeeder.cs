using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Authentications.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Database.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check if admin already exists
        if (await context.Users.AnyAsync(u => u.Username == "admin"))
            return;

        // Create admin user
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FullName = "System Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

        // Assign Admin role
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null)
        {
            var userRole = new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id };
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();
        }
    }
}
