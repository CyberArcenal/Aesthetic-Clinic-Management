using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.Repositories;
using System.Linq.Expressions;

namespace AestheticClinicAPI.Modules.Notifications.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly INotificationTemplateRepository _templateRepo;

        public NotificationTemplateService(INotificationTemplateRepository templateRepo)
        {
            _templateRepo = templateRepo;
        }

        private static NotificationTemplateResponseDto MapToDto(NotificationTemplate template) => new()
        {
            Id = template.Id,
            Name = template.Name,
            Subject = template.Subject,
            Content = template.Content,
            CreatedAt = template.CreatedAt
        };

        public async Task<ServiceResult<IEnumerable<NotificationTemplateResponseDto>>> GetAllAsync()
        {
            var templates = await _templateRepo.GetAllAsync();
            var dtos = templates.Select(MapToDto);
            return ServiceResult<IEnumerable<NotificationTemplateResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<NotificationTemplateResponseDto>> GetByIdAsync(int id)
        {
            var template = await _templateRepo.GetByIdAsync(id);
            if (template == null)
                return ServiceResult<NotificationTemplateResponseDto>.Failure("Template not found.");
            return ServiceResult<NotificationTemplateResponseDto>.Success(MapToDto(template));
        }

        public async Task<ServiceResult<NotificationTemplateResponseDto>> GetByNameAsync(string name)
        {
            var template = await _templateRepo.GetByNameAsync(name);
            if (template == null)
                return ServiceResult<NotificationTemplateResponseDto>.Failure("Template not found.");
            return ServiceResult<NotificationTemplateResponseDto>.Success(MapToDto(template));
        }

        public async Task<ServiceResult<NotificationTemplateResponseDto>> CreateAsync(CreateNotificationTemplateDto dto)
        {
            var template = new NotificationTemplate
            {
                Name = dto.Name,
                Subject = dto.Subject,
                Content = dto.Content
            };
            var created = await _templateRepo.AddAsync(template);
            return ServiceResult<NotificationTemplateResponseDto>.Success(MapToDto(created));
        }

        public async Task<ServiceResult<NotificationTemplateResponseDto>> UpdateAsync(int id, UpdateNotificationTemplateDto dto)
        {
            var template = await _templateRepo.GetByIdAsync(id);
            if (template == null)
                return ServiceResult<NotificationTemplateResponseDto>.Failure("Template not found.");

            if (!string.IsNullOrEmpty(dto.Name)) template.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Subject)) template.Subject = dto.Subject;
            if (!string.IsNullOrEmpty(dto.Content)) template.Content = dto.Content;

            await _templateRepo.UpdateAsync(template);
            return ServiceResult<NotificationTemplateResponseDto>.Success(MapToDto(template));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var template = await _templateRepo.GetByIdAsync(id);
            if (template == null)
                return ServiceResult<bool>.Failure("Template not found.");
            await _templateRepo.DeleteAsync(template);
            return ServiceResult<bool>.Success(true);
        }
        public async Task<ServiceResult<PaginatedResult<NotificationTemplateResponseDto>>> GetPaginatedAsync(int page, int pageSize, string? search = null)
        {
            Expression<Func<NotificationTemplate, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = t => t.Name.Contains(search) || t.Subject.Contains(search);
            }
            var paginated = await _templateRepo.GetPaginatedAsync(page, pageSize, filter);
            var dtos = paginated.Items.Select(MapToDto);
            var result = new PaginatedResult<NotificationTemplateResponseDto>
            {
                Items = dtos,
                Page = paginated.Page,
                PageSize = paginated.PageSize,
                TotalCount = paginated.TotalCount
            };
            return ServiceResult<PaginatedResult<NotificationTemplateResponseDto>>.Success(result);
        }
    }
}