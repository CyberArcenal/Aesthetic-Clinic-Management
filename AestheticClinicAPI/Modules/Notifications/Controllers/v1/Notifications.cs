using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AestheticClinicAPI.Modules.Notifications.Controllers.v1
{
    [ApiController]
    [Route("api/v1/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<
            ActionResult<ApiResponse<IEnumerable<NotificationResponseDto>>>
        > GetByUser(int userId, [FromQuery] int limit = 20)
        {
            var result = await _notificationService.GetByUserAsync(userId, limit);
            return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<
            ActionResult<ApiResponse<IEnumerable<NotificationResponseDto>>>
        > GetUnreadByUser(int userId)
        {
            var result = await _notificationService.GetUnreadByUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(int userId)
        {
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<int>.Ok(result.Data!));
        }

        [HttpPatch("{id}/read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Marked as read."));
        }

        [HttpPost("user/{userId}/read-all")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead(int userId)
        {
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse<bool>.Ok(true, "All marked as read."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _notificationService.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Notification deleted."));
        }
    }
}
