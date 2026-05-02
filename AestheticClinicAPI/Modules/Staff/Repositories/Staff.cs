using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Staff.Models;

namespace AestheticClinicAPI.Modules.Staff.Repositories
{
    public class StaffRepository : Repository<StaffMember>, IStaffRepository
    {
        public StaffRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<StaffMember>> GetActiveAsync()
        {
            return await _dbSet.Where(s => s.IsActive && !s.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<StaffMember>> GetByPositionAsync(string position)
        {
            return await _dbSet.Where(s => s.Position == position && !s.IsDeleted).ToListAsync();
        }
    }
}