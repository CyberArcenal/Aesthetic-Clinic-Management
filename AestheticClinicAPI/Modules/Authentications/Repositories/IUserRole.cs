using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Repositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId);
        Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    }
}