using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Staff.DTOs;

namespace AestheticClinicAPI.Modules.Staff.Services
{
    public interface IStaffService
    {
        Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetAllAsync();
        Task<ServiceResult<StaffResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<StaffResponseDto>> CreateAsync(CreateStaffDto dto);
        Task<ServiceResult<StaffResponseDto>> UpdateAsync(int id, UpdateStaffDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> ToggleActiveAsync(int id);
        Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetActiveAsync();
        Task<ServiceResult<IEnumerable<StaffResponseDto>>> GetByPositionAsync(string position);
        Task<ServiceResult<PaginatedResult<StaffResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null);
    }
}