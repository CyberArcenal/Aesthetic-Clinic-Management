using System.Text.Json;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.Repositories;
using AestheticClinicAPI.Modules.Notifications.Constants;
using AestheticClinicAPI.Modules.Notifications.Channels;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Notifications.Services
{
    public class NotifyLogService : INotifyLogService
    {
        private readonly INotifyLogRepository _logRepo;
        private readonly INotificationTemplateService _templateService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IPushService _pushService;

        public NotifyLogService(
            INotifyLogRepository logRepo,
            INotificationTemplateService templateService,
            IEmailService emailService,
            ISmsService smsService,
            IPushService pushService)
        {
            _logRepo = logRepo;
            _templateService = templateService;
            _emailService = emailService;
            _smsService = smsService;
            _pushService = pushService;
        }

        private NotifyLogResponseDto MapToDto(NotifyLog log)
        {
            return new NotifyLogResponseDto
            {
                Id = log.Id,
                RecipientEmail = log.RecipientEmail,
                Subject = log.Subject,
                Payload = log.Payload,
                Type = log.Type,
                Status = log.Status,
                ErrorMessage = log.ErrorMessage,
                Channel = log.Channel,
                MessageId = log.MessageId,
                DurationMs = log.DurationMs,
                SentAt = log.SentAt,
                CreatedAt = log.CreatedAt
            };
        }

        private async Task<string?> RenderTemplate(string templateName, Dictionary<string, string>? metadata)
        {
            var templateResult = await _templateService.GetByNameAsync(templateName);
            if (!templateResult.IsSuccess) return null;

            var content = templateResult.Data!.Content;
            if (metadata != null)
            {
                foreach (var kv in metadata)
                    content = content.Replace($"{{{{ {kv.Key} }}}}", kv.Value);
            }
            return content;
        }
        private async Task<(bool success, string subject, string body)> SendEmail(NotifyLog log)
        {
            var subject = log.Subject ?? "";
            var body = log.Payload ?? "";

            if (!string.IsNullOrEmpty(log.Type) && log.Type != "custom")
            {
                var templateResult = await _templateService.GetByNameAsync(log.Type);
                if (templateResult.IsSuccess)
                {
                    var metadata = log.Metadata != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(log.Metadata) : null;
                    subject = templateResult.Data!.Subject;
                    if (metadata != null)
                        foreach (var kv in metadata)
                            subject = subject.Replace($"{{{{ {kv.Key} }}}}", kv.Value);

                    body = templateResult.Data.Content;
                    if (metadata != null)
                        foreach (var kv in metadata)
                            body = body.Replace($"{{{{ {kv.Key} }}}}", kv.Value);
                }
                else
                {
                    throw new Exception($"Template '{log.Type}' not found.");
                }
            }

            var success = await _emailService.SendSimpleEmailAsync(log.RecipientEmail, subject, body);
            return (success, subject, body);
        }

        private async Task<bool> SendSms(NotifyLog log)
        {
            return await _smsService.SendSmsAsync(log.RecipientEmail, log.Payload ?? "");
        }

        private async Task<bool> SendPush(NotifyLog log)
        {
            return await _pushService.SendPushAsync(log.RecipientEmail, log.Subject ?? "Notification", log.Payload ?? "");
        }

        public async Task<ServiceResult<NotifyLogResponseDto>> CreateAsync(QueueNotificationDto dto)
        {
            // Create log entry
            var log = new NotifyLog
            {
                RecipientEmail = dto.Recipient,
                Subject = dto.Subject,
                Payload = dto.Message,
                Type = string.IsNullOrEmpty(dto.Type) ? "custom" : dto.Type,
                Channel = dto.Channel,
                Status = "Queued",
                Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null,
                CreatedAt = DateTime.UtcNow
            };
            var created = await _logRepo.AddAsync(log);

            // Perform sending
            var startTime = DateTime.UtcNow;
            bool success = false;
            string? error = null;
            string sentSubject = log.Subject ?? "";
            string sentPayload = log.Payload ?? "";

            try
            {
                switch (log.Channel.ToLower())
                {
                    case "email":
                        var emailResult = await SendEmail(log);
                        success = emailResult.success;
                        sentSubject = emailResult.subject;
                        sentPayload = emailResult.body;
                        break;
                    case "sms":
                        success = await SendSms(log);
                        sentPayload = log.Payload ?? "";
                        break;
                    case "push":
                        success = await SendPush(log);
                        sentPayload = log.Payload ?? "";
                        break;
                    default:
                        error = $"Unknown channel '{log.Channel}'";
                        success = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                success = false;
            }

            var durationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Update log
            created.Status = success ? "Sent" : "Failed";
            created.DurationMs = durationMs;
            if (success)
                created.SentAt = DateTime.UtcNow;
            else
                created.ErrorMessage = error;
            if (!string.IsNullOrEmpty(sentSubject)) created.Subject = sentSubject;
            if (!string.IsNullOrEmpty(sentPayload)) created.Payload = sentPayload;

            await _logRepo.UpdateAsync(created);
            return ServiceResult<NotifyLogResponseDto>.Success(MapToDto(created));
        }

        public async Task<ServiceResult<IEnumerable<NotifyLogResponseDto>>> GetAllAsync(string? status = null)
        {
            IEnumerable<NotifyLog> logs;
            if (!string.IsNullOrEmpty(status))
                logs = await _logRepo.GetByStatusAsync(status);
            else
                logs = await _logRepo.GetAllAsync();
            var dtos = logs.Select(MapToDto);
            return ServiceResult<IEnumerable<NotifyLogResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<PaginatedResult<NotifyLogResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? recipientEmail = null, string? status = null, string? channel = null)
        {
            Expression<Func<NotifyLog, bool>>? filter = null;
            if (!string.IsNullOrEmpty(recipientEmail) || !string.IsNullOrEmpty(status) || !string.IsNullOrEmpty(channel))
            {
                filter = l => (string.IsNullOrEmpty(recipientEmail) || l.RecipientEmail.Contains(recipientEmail))
                           && (string.IsNullOrEmpty(status) || l.Status == status)
                           && (string.IsNullOrEmpty(channel) || l.Channel == channel);
            }
            var paginated = await _logRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = paginated.Items.Select(MapToDto);
            var result = new PaginatedResult<NotifyLogResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<NotifyLogResponseDto>>.Success(result);
        }

        public async Task<ServiceResult<NotifyLogResponseDto>> GetByIdAsync(int id)
        {
            var log = await _logRepo.GetByIdAsync(id);
            if (log == null)
                return ServiceResult<NotifyLogResponseDto>.Failure("Log not found.");
            return ServiceResult<NotifyLogResponseDto>.Success(MapToDto(log));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var log = await _logRepo.GetByIdAsync(id);
            if (log == null)
                return ServiceResult<bool>.Failure("Log not found.");
            await _logRepo.DeleteAsync(log);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> RetryAsync(int id)
        {
            var log = await _logRepo.GetByIdAsync(id);
            if (log == null)
                return ServiceResult<bool>.Failure("Log not found.");
            if (log.Status != "Failed")
                return ServiceResult<bool>.Failure("Only failed logs can be retried.");

            // Resend
            var startTime = DateTime.UtcNow;
            bool success = false;
            string? error = null;
            string sentSubject = log.Subject ?? "";
            string sentPayload = log.Payload ?? "";

            try
            {
                switch (log.Channel.ToLower())
                {
                    case "email":
                        var emailResult = await SendEmail(log);
                        success = emailResult.success;
                        sentSubject = emailResult.subject;
                        sentPayload = emailResult.body;
                        break;
                    case "sms":
                        success = await SendSms(log);
                        sentPayload = log.Payload ?? "";
                        break;
                    case "push":
                        success = await SendPush(log);
                        sentPayload = log.Payload ?? "";
                        break;
                    default:
                        error = $"Unknown channel '{log.Channel}'";
                        success = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                success = false;
            }

            var durationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            log.Status = success ? "Sent" : "Failed";
            log.DurationMs = durationMs;
            if (success)
                log.SentAt = DateTime.UtcNow;
            else
                log.ErrorMessage = error;
            if (!string.IsNullOrEmpty(sentSubject)) log.Subject = sentSubject;
            if (!string.IsNullOrEmpty(sentPayload)) log.Payload = sentPayload;

            await _logRepo.UpdateAsync(log);
            return ServiceResult<bool>.Success(true);
        }
    }
}