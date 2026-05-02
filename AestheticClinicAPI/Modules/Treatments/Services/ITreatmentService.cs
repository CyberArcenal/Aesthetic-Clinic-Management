using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Treatments.DTOs;

namespace AestheticClinicAPI.Modules.Treatments.Services
{
    public interface ITreatmentService
    {
        Task<ServiceResult<IEnumerable<TreatmentResponseDto>>> GetAllAsync();
        Task<ServiceResult<TreatmentResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<TreatmentResponseDto>> CreateAsync(CreateTreatmentDto dto);
        Task<ServiceResult<TreatmentResponseDto>> UpdateAsync(int id, UpdateTreatmentDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> ToggleActiveAsync(int id);
        Task<ServiceResult<IEnumerable<TreatmentResponseDto>>> GetByCategoryAsync(string category);
        Task<ServiceResult<IEnumerable<TreatmentResponseDto>>> GetActiveAsync();
        Task<ServiceResult<decimal>> GetTotalRevenueAsync();
        Task<ServiceResult<PaginatedResult<TreatmentResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null);
    }
}