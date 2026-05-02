using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;

namespace AestheticClinicAPI.Modules.Notifications.Services
{
    public interface INotificationService
    {
        Task<ServiceResult<IEnumerable<NotificationResponseDto>>> GetByUserAsync(int userId, int limit = 20);
        Task<ServiceResult<IEnumerable<NotificationResponseDto>>> GetUnreadByUserAsync(int userId);
        Task<ServiceResult<int>> GetUnreadCountAsync(int userId);
        Task<ServiceResult<bool>> MarkAsReadAsync(int notificationId);
        Task<ServiceResult<bool>> MarkAllAsReadAsync(int userId);
        Task<ServiceResult<bool>> DeleteAsync(int notificationId);
        Task<ServiceResult<NotificationResponseDto>> CreateAsync(CreateNotificationDto dto);
        
    }
}