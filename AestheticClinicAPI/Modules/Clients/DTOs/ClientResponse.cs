namespace AestheticClinicAPI.Modules.Clients.DTOs
{
    public class ClientResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? SkinHistory { get; set; }
        public string? Allergies { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}