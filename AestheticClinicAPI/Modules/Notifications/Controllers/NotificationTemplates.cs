using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Services;

namespace AestheticClinicAPI.Modules.Notifications.Controllers
{
    [ApiController]
    [Route("api/v1/notification-templates")]
    public class NotificationTemplatesController : ControllerBase
    {
        private readonly INotificationTemplateService _templateService;

        public NotificationTemplatesController(INotificationTemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<NotificationTemplateResponseDto>>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _templateService.GetPaginatedAsync(page, pageSize, search);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<NotificationTemplateResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<NotificationTemplateResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<NotificationTemplateResponseDto>>> GetById(int id)
        {
            var result = await _templateService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<NotificationTemplateResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<NotificationTemplateResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<NotificationTemplateResponseDto>>> Create([FromBody] CreateNotificationTemplateDto dto)
        {
            var result = await _templateService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<NotificationTemplateResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<NotificationTemplateResponseDto>.Ok(result.Data!, "Template created."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<NotificationTemplateResponseDto>>> Update(int id, [FromBody] UpdateNotificationTemplateDto dto)
        {
            var result = await _templateService.UpdateAsync(id, dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<NotificationTemplateResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<NotificationTemplateResponseDto>.Ok(result.Data!, "Template updated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _templateService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Template deleted."));
        }
    }
}