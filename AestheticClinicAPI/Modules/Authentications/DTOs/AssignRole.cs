using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Authentications.DTOs
{
    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}