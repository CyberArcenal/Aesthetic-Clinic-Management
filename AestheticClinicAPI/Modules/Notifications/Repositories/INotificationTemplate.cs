using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.Models;

namespace AestheticClinicAPI.Modules.Notifications.Repositories
{
    public interface INotificationTemplateRepository : IRepository<NotificationTemplate>
    {
        Task<NotificationTemplate?> GetByNameAsync(string name);
    }
}