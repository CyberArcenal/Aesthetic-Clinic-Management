using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public interface IUserRoleService
    {
        Task<ServiceResult<IEnumerable<UserRoleResponseDto>>> GetAllAsync();
        Task<ServiceResult<IEnumerable<UserRoleResponseDto>>> GetByUserIdAsync(int userId);
        Task<ServiceResult<IEnumerable<UserRoleResponseDto>>> GetByRoleIdAsync(int roleId);
        Task<ServiceResult<IEnumerable<string>>> GetUserRolesAsync(int userId);
        Task<ServiceResult<bool>> AssignRoleAsync(AssignRoleDto dto);
        Task<ServiceResult<bool>> RemoveRoleAsync(AssignRoleDto dto);
        Task<ServiceResult<bool>> UserHasRoleAsync(int userId, string roleName);
    }
}