using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Appointments.DTOs;

namespace AestheticClinicAPI.Modules.Appointments.Services
{
    public interface IAppointmentService
    {
        Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetAllAsync();
        Task<ServiceResult<AppointmentResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<AppointmentResponseDto>> CreateAsync(CreateAppointmentDto dto);
        Task<ServiceResult<AppointmentResponseDto>> UpdateAsync(int id, UpdateAppointmentDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> UpdateStatusAsync(int id, string newStatus);
        Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetByClientAsync(int clientId);
        Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<ServiceResult<bool>> CheckAvailabilityAsync(int staffId, DateTime startTime, int durationMinutes);
        Task<ServiceResult<PaginatedResult<AppointmentResponseDto>>> GetPaginatedAsync(
        int page,
        int pageSize,
        int? clientId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    }
}