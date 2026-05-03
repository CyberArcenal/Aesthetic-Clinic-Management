using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public interface IRoleService
    {
        Task<ServiceResult<IEnumerable<RoleResponseDto>>> GetAllAsync();
        Task<ServiceResult<RoleResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<RoleResponseDto>> GetByNameAsync(string name);
        Task<ServiceResult<RoleResponseDto>> CreateAsync(CreateRoleDto dto);
        Task<ServiceResult<RoleResponseDto>> UpdateAsync(int id, UpdateRoleDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
