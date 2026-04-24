namespace AestheticClinicAPI.Modules.Notifications.DTOs
{
    public class NotifyLogResponseDto
    {
        public int Id { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Payload { get; set; }
        public string? Type { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public int? DurationMs { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}