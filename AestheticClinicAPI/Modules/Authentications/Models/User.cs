using AestheticClinicAPI.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.Models
{
    public class User : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;  // store hashed password

        [MaxLength(100)]
        public string? FullName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }
    }
}