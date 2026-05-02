using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;

namespace AestheticClinicAPI.Modules.Notifications.Services
{
    public interface INotifyLogService
    {
        Task<ServiceResult<IEnumerable<NotifyLogResponseDto>>> GetAllAsync(string? status = null);
        Task<ServiceResult<NotifyLogResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<NotifyLogResponseDto>> CreateAsync(QueueNotificationDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> RetryAsync(int id);
        Task<ServiceResult<PaginatedResult<NotifyLogResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? recipientEmail = null, string? status = null, string? channel = null);
    }
}