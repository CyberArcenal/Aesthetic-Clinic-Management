using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Staff.DTOs
{
    public class StaffResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateStaffDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Position { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateStaffDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public bool? IsActive { get; set; }
    }
}