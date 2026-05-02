using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.Models;

namespace AestheticClinicAPI.Modules.Notifications.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(int userId)
        {
            return await _dbSet
                .Where(n => n.RecipientId == userId && !n.IsRead && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _dbSet
                .CountAsync(n => n.RecipientId == userId && !n.IsRead && !n.IsDeleted);
        }

        public async Task<IEnumerable<Notification>> GetByUserAsync(int userId, int limit = 20)
        {
            return await _dbSet
                .Where(n => n.RecipientId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _dbSet
                .Where(n => n.RecipientId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }
            if (unread.Any())
                await _context.SaveChangesAsync();
        }
    }
}