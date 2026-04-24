using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Notifications.DTOs
{
    public class QueueNotificationDto
    {
        [Required]
        public string Channel { get; set; } = "email"; // email, sms, push

        [Required]
        public string Recipient { get; set; } = string.Empty;

        public string? Subject { get; set; }
        public string? Message { get; set; }

        public string? Type { get; set; } // template name, or "custom"

        public object? Metadata { get; set; } // will be serialized to JSON
    }
}