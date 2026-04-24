using System.ComponentModel.DataAnnotations;
using AestheticClinicAPI.Modules.Shared;

namespace AestheticClinicAPI.Modules.Clients.Models
{
    public class Client : BaseEntity
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Phone, MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? SkinHistory { get; set; }
        public string? Allergies { get; set; }
    }
}