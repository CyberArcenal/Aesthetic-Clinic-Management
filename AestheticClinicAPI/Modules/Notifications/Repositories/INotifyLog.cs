using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.Models;

namespace AestheticClinicAPI.Modules.Notifications.Repositories
{
    public interface INotifyLogRepository : IRepository<NotifyLog>
    {
        Task<IEnumerable<NotifyLog>> GetByRecipientEmailAsync(string email);
        Task<IEnumerable<NotifyLog>> GetByStatusAsync(string status);
        Task<IEnumerable<NotifyLog>> GetFailedLogsAsync();
        Task<NotifyLog?> GetByMessageIdAsync(string messageId);
    }
}