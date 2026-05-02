using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AestheticClinicAPI.Modules.Photos.DTOs;

public class CreatePhotoDto
{
    [Required]
    public int ClientId { get; set; }

    public int? AppointmentId { get; set; }

    [Required]
    public bool IsBefore { get; set; } = true;

    public string? Description { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}