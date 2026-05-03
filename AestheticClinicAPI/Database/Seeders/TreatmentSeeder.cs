using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Treatments.Models;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Database.Seeders;

public static class TreatmentSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Treatments.AnyAsync())
            return;

        var treatments = new List<Treatment>
        {
            new Treatment
            {
                Name = "HydraFacial",
                Category = "Facial",
                DurationMinutes = 60,
                Price = 3500,
                IsActive = true,
            },
            new Treatment
            {
                Name = "Botox (per area)",
                Category = "Injectable",
                DurationMinutes = 30,
                Price = 8000,
                IsActive = true,
            },
            new Treatment
            {
                Name = "Laser Hair Removal (small)",
                Category = "Laser",
                DurationMinutes = 30,
                Price = 2500,
                IsActive = true,
            },
            new Treatment
            {
                Name = "Chemical Peel",
                Category = "Facial",
                DurationMinutes = 45,
                Price = 4000,
                IsActive = true,
            },
            new Treatment
            {
                Name = "Dermaplaning",
                Category = "Facial",
                DurationMinutes = 45,
                Price = 3000,
                IsActive = true,
            },
            new Treatment
            {
                Name = "RF Microneedling",
                Category = "Laser",
                DurationMinutes = 60,
                Price = 12000,
                IsActive = true,
            },
            new Treatment
            {
                Name = "LED Light Therapy",
                Category = "Facial",
                DurationMinutes = 30,
                Price = 2000,
                IsActive = true,
            },
            new Treatment
            {
                Name = "PRP (Platelet-Rich Plasma)",
                Category = "Injectable",
                DurationMinutes = 60,
                Price = 15000,
                IsActive = true,
            },
        };

        await context.Treatments.AddRangeAsync(treatments);
        await context.SaveChangesAsync();
    }
}
