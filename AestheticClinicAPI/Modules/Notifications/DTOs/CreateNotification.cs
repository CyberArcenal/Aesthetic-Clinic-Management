using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Notifications.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public int RecipientId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "Info";
        public string Channel { get; set; } = "InApp";
        public string? ActionUrl { get; set; }
    }
}