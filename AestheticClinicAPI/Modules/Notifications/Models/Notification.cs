using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Notifications.Models
{
    public class Notification : BaseEntity
    {
        [Required]
        public int RecipientId { get; set; }  // UserId (from Authentications module)

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Type { get; set; } = "Info";  // Info, Success, Warning, Error

        [MaxLength(20)]
        public string Channel { get; set; } = "InApp";  // InApp, Email, SMS, Push

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }  // deep link / route
        [MaxLength(2000)]
        public string? Metadata { get; set; }  // JSON string for template context, etc.
    }
}