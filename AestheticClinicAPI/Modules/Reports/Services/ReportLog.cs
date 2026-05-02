using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Reports.DTOs;
using AestheticClinicAPI.Modules.Reports.Models;
using AestheticClinicAPI.Modules.Reports.Repositories;
using AestheticClinicAPI.Modules.Authentications.Services;
using System.Text.Json;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Reports.Services
{
    public class ReportLogService : IReportLogService
    {
        private readonly IReportLogRepository _reportLogRepo;
        private readonly IUserService _userService;

        public ReportLogService(IReportLogRepository reportLogRepo, IUserService userService)
        {
            _reportLogRepo = reportLogRepo;
            _userService = userService;
        }

        private async Task<ReportLogResponseDto> MapToDto(ReportLog log)
        {
            string? generatedByName = null;
            if (log.GeneratedById.HasValue)
            {
                var userResult = await _userService.GetByIdAsync(log.GeneratedById.Value);
                if (userResult.IsSuccess)
                    generatedByName = userResult.Data?.Username;
            }

            return new ReportLogResponseDto
            {
                Id = log.Id,
                ReportName = log.ReportName,
                Parameters = log.Parameters,
                GeneratedById = log.GeneratedById,
                GeneratedByName = generatedByName,
                Insights = log.Insights,
                GeneratedAt = log.GeneratedAt,
                CreatedAt = log.CreatedAt
            };
        }

        public async Task<ServiceResult<IEnumerable<ReportLogResponseDto>>> GetAllAsync(string? reportName = null)
        {
            IEnumerable<ReportLog> logs;
            if (!string.IsNullOrEmpty(reportName))
                logs = await _reportLogRepo.GetByReportNameAsync(reportName);
            else
                logs = await _reportLogRepo.GetAllAsync();

            var dtos = new List<ReportLogResponseDto>();
            foreach (var log in logs)
                dtos.Add(await MapToDto(log));

            return ServiceResult<IEnumerable<ReportLogResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<ReportLogResponseDto>> GetByIdAsync(int id)
        {
            var log = await _reportLogRepo.GetByIdAsync(id);
            if (log == null)
                return ServiceResult<ReportLogResponseDto>.Failure("Report log not found.");
            return ServiceResult<ReportLogResponseDto>.Success(await MapToDto(log));
        }

        public async Task<ServiceResult<ReportLogResponseDto>> GenerateReportAsync(GenerateReportDto dto)
        {
            // Create new report log entry
            var reportLog = new ReportLog
            {
                ReportName = dto.ReportName,
                Parameters = dto.Parameters,
                GeneratedAt = DateTime.UtcNow,
                // Insights can be generated later (e.g., via AI or external service)
                Insights = "Report generated. Insights will be added later."
            };

            var created = await _reportLogRepo.AddAsync(reportLog);
            return ServiceResult<ReportLogResponseDto>.Success(await MapToDto(created));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var log = await _reportLogRepo.GetByIdAsync(id);
            if (log == null)
                return ServiceResult<bool>.Failure("Report log not found.");
            await _reportLogRepo.DeleteAsync(log);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<PaginatedResult<ReportLogResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? reportName = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            Expression<Func<ReportLog, bool>>? filter = null;
            if (!string.IsNullOrEmpty(reportName) || fromDate.HasValue || toDate.HasValue)
            {
                filter = r => (string.IsNullOrEmpty(reportName) || r.ReportName.Contains(reportName))
                           && (!fromDate.HasValue || r.GeneratedAt >= fromDate.Value)
                           && (!toDate.HasValue || r.GeneratedAt <= toDate.Value);
            }
            var paginated = await _reportLogRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = new List<ReportLogResponseDto>();
            foreach (var log in paginated.Items)
                dtos.Add(await MapToDto(log));
            var result = new PaginatedResult<ReportLogResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<ReportLogResponseDto>>.Success(result);
        }
    }
}