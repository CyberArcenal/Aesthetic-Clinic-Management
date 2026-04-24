using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.DTOs
{
    public class CreateUserDto
    {
        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public IEnumerable<string>? Roles { get; set; }
    }
}