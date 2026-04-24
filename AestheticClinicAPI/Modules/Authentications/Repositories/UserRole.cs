using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Repositories
{
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Where(ur => ur.UserId == userId && !ur.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId)
        {
            return await _dbSet.Where(ur => ur.RoleId == roleId && !ur.IsDeleted).ToListAsync();
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            var hasRole = await _dbSet
                .Join(_context.Set<Role>(), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                .Where(x => x.ur.UserId == userId && x.r.Name == roleName && !x.ur.IsDeleted && !x.r.IsDeleted)
                .AnyAsync();
            return hasRole;
        }
        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            return await _dbSet
                .Where(ur => ur.UserId == userId)
                .Join(_context.Set<Role>(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();
        }
    }
}