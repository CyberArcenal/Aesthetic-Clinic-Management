using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Clients.DTOs
{
    public class CreateClientDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? SkinHistory { get; set; }
        public string? Allergies { get; set; }
    }
}