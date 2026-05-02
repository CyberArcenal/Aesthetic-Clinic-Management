using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Services;

namespace AestheticClinicAPI.Modules.Notifications.Controllers
{
    [ApiController]
    [Route("api/v1/notify-logs")]
    public class NotifyLogsController : ControllerBase
    {
        private readonly INotifyLogService _notifyLogService;

        public NotifyLogsController(INotifyLogService notifyLogService)
        {
            _notifyLogService = notifyLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<NotifyLogResponseDto>>>> GetAll(
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] string? recipientEmail = null,
          [FromQuery] string? status = null,
          [FromQuery] string? channel = null)
        {
            var result = await _notifyLogService.GetPaginatedAsync(page, pageSize, recipientEmail, status, channel);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<NotifyLogResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<NotifyLogResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<NotifyLogResponseDto>>> GetById(int id)
        {
            var result = await _notifyLogService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<NotifyLogResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<NotifyLogResponseDto>.Ok(result.Data!));
        }

        [HttpPost("{id}/retry")]
        public async Task<ActionResult<ApiResponse<bool>>> Retry(int id)
        {
            var result = await _notifyLogService.RetryAsync(id);
            if (!result.IsSuccess) return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Retry queued."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _notifyLogService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Log deleted."));
        }
    }
}