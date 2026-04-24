using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Treatments.Models;

namespace AestheticClinicAPI.Modules.Treatments.Repositories
{
    public class TreatmentRepository : Repository<Treatment>, ITreatmentRepository
    {
        public TreatmentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Treatment>> GetByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(t => t.Category == category && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Treatment>> GetActiveAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive && !t.IsDeleted)
                .ToListAsync();
        }
    }
}