using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;

namespace AestheticClinicAPI.Modules.Notifications.Services
{
    public interface INotificationTemplateService
    {
        Task<ServiceResult<IEnumerable<NotificationTemplateResponseDto>>> GetAllAsync();
        Task<ServiceResult<NotificationTemplateResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<NotificationTemplateResponseDto>> CreateAsync(CreateNotificationTemplateDto dto);
        Task<ServiceResult<NotificationTemplateResponseDto>> UpdateAsync(int id, UpdateNotificationTemplateDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<NotificationTemplateResponseDto>> GetByNameAsync(string name);

        Task<ServiceResult<PaginatedResult<NotificationTemplateResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null);
    }
}