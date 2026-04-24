namespace AestheticClinicAPI.Modules.Clients.DTOs
{
    public class UpdateClientDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? SkinHistory { get; set; }
        public string? Allergies { get; set; }
    }
}