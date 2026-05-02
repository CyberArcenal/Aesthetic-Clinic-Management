using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.Models;

namespace AestheticClinicAPI.Modules.Notifications.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadByUserAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<IEnumerable<Notification>> GetByUserAsync(int userId, int limit = 20);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }
}