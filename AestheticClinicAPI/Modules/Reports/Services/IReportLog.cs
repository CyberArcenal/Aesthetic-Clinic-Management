using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Reports.DTOs;

namespace AestheticClinicAPI.Modules.Reports.Services
{
    public interface IReportLogService
    {
        Task<ServiceResult<IEnumerable<ReportLogResponseDto>>> GetAllAsync(string? reportName = null);
        Task<ServiceResult<ReportLogResponseDto>> GetByIdAsync(int id);
        Task<ServiceResult<ReportLogResponseDto>> GenerateReportAsync(GenerateReportDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);

        Task<ServiceResult<PaginatedResult<ReportLogResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? reportName = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}