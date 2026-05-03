using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Photos.DTOs;
using AestheticClinicAPI.Modules.Photos.Services;

namespace AestheticClinicAPI.Modules.Photos.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;

    public PhotosController(IPhotoService photoService)
    {
        _photoService = photoService;
    }

    // GET endpoints (no changes needed)
    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhotoResponseDto>>>> GetByClient(int clientId)
    {
        var result = await _photoService.GetByClientAsync(clientId);
        return Ok(ApiResponse<IEnumerable<PhotoResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("client/{clientId}/before")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhotoResponseDto>>>> GetBeforePhotos(int clientId)
    {
        var result = await _photoService.GetBeforePhotosAsync(clientId);
        return Ok(ApiResponse<IEnumerable<PhotoResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("client/{clientId}/after")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhotoResponseDto>>>> GetAfterPhotos(int clientId)
    {
        var result = await _photoService.GetAfterPhotosAsync(clientId);
        return Ok(ApiResponse<IEnumerable<PhotoResponseDto>>.Ok(result.Data!));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PhotoResponseDto>>> GetById(int id)
    {
        var result = await _photoService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(ApiResponse<PhotoResponseDto>.Fail(result.ErrorMessage!));
        return Ok(ApiResponse<PhotoResponseDto>.Ok(result.Data!));
    }

    // ✨ CHANGED: from [FromBody] to [FromForm]
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<ApiResponse<PhotoResponseDto>>> Create([FromForm] CreatePhotoDto dto)
    {
        var result = await _photoService.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(ApiResponse<PhotoResponseDto>.Fail(result.ErrorMessage!));
        return Ok(ApiResponse<PhotoResponseDto>.Ok(result.Data!, "Photo uploaded successfully."));
    }

    // ✨ NEW: endpoint para i-serve ang image file (kung ayaw mong gumamit ng static files)
    [HttpGet("file/{id}")]
    [AllowAnonymous] // or [Authorize] kung gusto mong protektahan
    public async Task<IActionResult> GetPhotoFile(int id)
    {
        var result = await _photoService.GetPhotoFileAsync(id);
        if (!result.IsSuccess)
            return NotFound();
        return File(result.Data!.FileBytes, result.Data!.MimeType, result.Data!.FileName);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _photoService.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
        return Ok(ApiResponse<bool>.Ok(true, "Photo deleted."));
    }
}