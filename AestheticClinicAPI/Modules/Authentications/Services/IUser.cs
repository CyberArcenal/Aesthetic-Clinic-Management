using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Models;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public interface IUserService
    {
        Task<ServiceResult<IEnumerable<UserResponseDto>>> GetAllAsync();
        Task<ServiceResult<UserResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<UserResponseDto>> CreateAsync(CreateUserDto dto);
        Task<ServiceResult<UserResponseDto>> UpdateAsync(int id, UpdateUserDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> ActivateAsync(int id, bool isActive);
        Task<ServiceResult<IEnumerable<string>>> GetUserRolesAsync(int userId);
        Task<ServiceResult<PaginatedResult<UserResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null);
    }
}