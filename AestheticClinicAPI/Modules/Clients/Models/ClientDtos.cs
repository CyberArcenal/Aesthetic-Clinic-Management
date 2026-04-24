using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Clients.Models
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

    public class UpdateClientDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? SkinHistory { get; set; }
        public string? Allergies { get; set; }
    }
}