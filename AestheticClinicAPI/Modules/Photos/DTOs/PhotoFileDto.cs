namespace AestheticClinicAPI.Modules.Photos.DTOs;

public class PhotoFileDto
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}