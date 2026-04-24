using AestheticClinicAPI.Modules.Shared;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Staff.Models
{
    public class StaffMember : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [Phone, MaxLength(20)]
        public string? Phone { get; set; }

        public string? Position { get; set; }

        public bool IsActive { get; set; } = true;
    }
}