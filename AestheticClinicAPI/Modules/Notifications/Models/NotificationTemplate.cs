using AestheticClinicAPI.Modules.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Notifications.Models
{
    public class NotificationTemplate : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;  // e.g., "AppointmentReminder", "InvoicePaid"

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;  // supports placeholders like {{ClientName}}
    }
}