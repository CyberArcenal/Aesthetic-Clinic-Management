using Microsoft.EntityFrameworkCore;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Reports.Models;

namespace AestheticClinicAPI.Modules.Reports.Repositories
{
    public class ReportLogRepository : Repository<ReportLog>, IReportLogRepository
    {
        public ReportLogRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ReportLog>> GetByReportNameAsync(string reportName)
        {
            return await _dbSet
                .Where(r => r.ReportName == reportName && !r.IsDeleted)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReportLog>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _dbSet
                .Where(r => r.GeneratedAt >= start && r.GeneratedAt <= end && !r.IsDeleted)
                .OrderBy(r => r.GeneratedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReportLog>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.GeneratedById == userId && !r.IsDeleted)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();
        }
    }
}