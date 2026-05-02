using AestheticClinicAPI.Data;

namespace AestheticClinicAPI.Database.Seeders;

public static class ApplicationDbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Order matters because of foreign keys
        await RoleSeeder.SeedAsync(context);      // Roles first
        await UserSeeder.SeedAsync(context);      // Admin user needs roles
        await TreatmentSeeder.SeedAsync(context); // Treatments independent
        await StaffSeeder.SeedAsync(context);     // Optional staff
    }
}