using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Reports.Models;

namespace AestheticClinicAPI.Modules.Reports.Repositories
{
    public interface IReportLogRepository : IRepository<ReportLog>
    {
        Task<IEnumerable<ReportLog>> GetByReportNameAsync(string reportName);
        Task<IEnumerable<ReportLog>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<ReportLog>> GetByUserAsync(int userId);
    }
}