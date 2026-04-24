namespace AestheticClinicAPI.Modules.Notifications.DTOs
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public int RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}