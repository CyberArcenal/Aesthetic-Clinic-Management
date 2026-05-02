using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Notifications.Models
{
    public class NotifyLog : BaseEntity
    {
        [Required, EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Subject { get; set; }

        public string? Payload { get; set; }  // the actual message sent (plain or rendered template)

        [MaxLength(50)]
        public string? Type { get; set; }   // template name or "custom"

        [MaxLength(20)]
        public string Status { get; set; } = "Queued";  // Queued, Sent, Failed, Resend

        public string? ErrorMessage { get; set; }

        [MaxLength(50)]
        public string Channel { get; set; } = "Email";  // Email, SMS, Push

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        [MaxLength(255)]
        public string? MessageId { get; set; }  // from external provider (e.g., SendGrid, Twilio)

        public int? DurationMs { get; set; }  // time taken to send

        public int RetryCount { get; set; } = 0;
        public int ResendCount { get; set; } = 0;
        public DateTime? SentAt { get; set; }
        public DateTime? LastErrorAt { get; set; }
        [MaxLength(2000)]
        public string? Metadata { get; set; }  // JSON string for template context, etc.
    }
}