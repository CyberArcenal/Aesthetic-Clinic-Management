using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Authentications.Models;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Database.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = new List<Role>
        {
            new Role { Name = "Admin", Description = "Full system access" },
            new Role
            {
                Name = "Staff",
                Description = "Regular staff access (view, create, update)",
            },
            new Role { Name = "Client", Description = "Client portal access (view own data only)" },
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
