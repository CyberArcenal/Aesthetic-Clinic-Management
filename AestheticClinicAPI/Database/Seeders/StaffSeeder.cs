using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Staff.Models;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Database.Seeders;

public static class StaffSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.StaffMembers.AnyAsync()) return;

        var staffMembers = new List<StaffMember>
        {
            new StaffMember { Name = "Dr. Maria Santos", Email = "maria.santos@clinic.com", Phone = "+639171234567", Position = "Dermatologist", IsActive = true },
            new StaffMember { Name = "Nurse Jane Dela Cruz", Email = "jane.delacruz@clinic.com", Phone = "+639172345678", Position = "Nurse", IsActive = true },
            new StaffMember { Name = "Receptionist Ana Reyes", Email = "ana.reyes@clinic.com", Phone = "+639173456789", Position = "Receptionist", IsActive = true }
        };

        await context.StaffMembers.AddRangeAsync(staffMembers);
        await context.SaveChangesAsync();
    }
}