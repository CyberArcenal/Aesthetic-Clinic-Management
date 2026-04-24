using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.DTOs
{
    public class CreateRoleDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}