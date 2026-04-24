using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Notifications.Models;

namespace AestheticClinicAPI.Modules.Notifications.Repositories
{
    public class NotifyLogRepository : Repository<NotifyLog>, INotifyLogRepository
    {
        public NotifyLogRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<NotifyLog>> GetByRecipientEmailAsync(string email)
        {
            return await _dbSet
                .Where(l => l.RecipientEmail == email && !l.IsDeleted)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotifyLog>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(l => l.Status == status && !l.IsDeleted)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotifyLog>> GetFailedLogsAsync()
        {
            return await _dbSet
                .Where(l => l.Status == "Failed" && !l.IsDeleted)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<NotifyLog?> GetByMessageIdAsync(string messageId)
        {
            return await _dbSet.FirstOrDefaultAsync(l => l.MessageId == messageId && !l.IsDeleted);
        }
    }
}