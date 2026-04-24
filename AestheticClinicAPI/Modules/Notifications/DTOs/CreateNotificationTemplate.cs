using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Notifications.DTOs
{
    public class CreateNotificationTemplateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}