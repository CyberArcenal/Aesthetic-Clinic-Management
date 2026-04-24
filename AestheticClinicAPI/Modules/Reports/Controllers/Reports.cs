using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Reports.DTOs;
using AestheticClinicAPI.Modules.Reports.Services;

namespace AestheticClinicAPI.Modules.Reports.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportLogService _reportLogService;

        public ReportsController(IReportLogService reportLogService)
        {
            _reportLogService = reportLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ReportLogResponseDto>>>> GetAll(
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10,
         [FromQuery] string? reportName = null,
         [FromQuery] DateTime? fromDate = null,
         [FromQuery] DateTime? toDate = null)
        {
            var result = await _reportLogService.GetPaginatedAsync(page, pageSize, reportName, fromDate, toDate);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<ReportLogResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<ReportLogResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ReportLogResponseDto>>> GetById(int id)
        {
            var result = await _reportLogService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<ReportLogResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<ReportLogResponseDto>.Ok(result.Data!));
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ApiResponse<ReportLogResponseDto>>> GenerateReport([FromBody] GenerateReportDto dto)
        {
            var result = await _reportLogService.GenerateReportAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<ReportLogResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<ReportLogResponseDto>.Ok(result.Data!, "Report generated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _reportLogService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Report deleted."));
        }
    }
}