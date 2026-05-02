using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Repositories
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext context) : base(context) { }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.Name == name && !r.IsDeleted);
        }
    }
}