using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Services;

namespace AestheticClinicAPI.Modules.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserService _userService;

        public NotificationService(INotificationRepository notificationRepo, IUserService userService)
        {
            _notificationRepo = notificationRepo;
            _userService = userService;
        }

        private async Task<NotificationResponseDto> MapToDto(Notification notification)
        {
            var userResult = await _userService.GetByIdAsync(notification.RecipientId);
            return new NotificationResponseDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                RecipientName = userResult.IsSuccess ? userResult.Data?.Username : null,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Channel = notification.Channel,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                ActionUrl = notification.ActionUrl,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task<ServiceResult<IEnumerable<NotificationResponseDto>>> GetByUserAsync(int userId, int limit = 20)
        {
            var notifications = await _notificationRepo.GetByUserAsync(userId, limit);
            var dtos = new List<NotificationResponseDto>();
            foreach (var n in notifications)
                dtos.Add(await MapToDto(n));
            return ServiceResult<IEnumerable<NotificationResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IEnumerable<NotificationResponseDto>>> GetUnreadByUserAsync(int userId)
        {
            var notifications = await _notificationRepo.GetUnreadByUserAsync(userId);
            var dtos = new List<NotificationResponseDto>();
            foreach (var n in notifications)
                dtos.Add(await MapToDto(n));
            return ServiceResult<IEnumerable<NotificationResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<int>> GetUnreadCountAsync(int userId)
        {
            var count = await _notificationRepo.GetUnreadCountAsync(userId);
            return ServiceResult<int>.Success(count);
        }

        public async Task<ServiceResult<bool>> MarkAsReadAsync(int notificationId)
        {
            await _notificationRepo.MarkAsReadAsync(notificationId);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> MarkAllAsReadAsync(int userId)
        {
            await _notificationRepo.MarkAllAsReadAsync(userId);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int notificationId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null)
                return ServiceResult<bool>.Failure("Notification not found.");
            await _notificationRepo.DeleteAsync(notification);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<NotificationResponseDto>> CreateAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                RecipientId = dto.RecipientId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Channel = dto.Channel,
                ActionUrl = dto.ActionUrl,
                IsRead = false
            };
            var created = await _notificationRepo.AddAsync(notification);
            return ServiceResult<NotificationResponseDto>.Success(await MapToDto(created));
        }
    }
}