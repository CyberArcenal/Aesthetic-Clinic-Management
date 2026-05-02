using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.Models;

namespace AestheticClinicAPI.Modules.Notifications.Repositories
{
    public class NotificationTemplateRepository : Repository<NotificationTemplate>, INotificationTemplateRepository
    {
        public NotificationTemplateRepository(AppDbContext context) : base(context) { }

        public async Task<NotificationTemplate?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted);
        }
    }
}