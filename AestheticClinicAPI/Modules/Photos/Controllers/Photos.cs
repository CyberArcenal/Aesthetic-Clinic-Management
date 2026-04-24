using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Photos.DTOs;
using AestheticClinicAPI.Modules.Photos.Services;

namespace AestheticClinicAPI.Modules.Photos.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PhotoResponseDto>>> Create([FromBody] CreatePhotoDto dto)
        {
            var result = await _photoService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<PhotoResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PhotoResponseDto>.Ok(result.Data!, "Photo uploaded."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _photoService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Photo deleted."));
        }
    }
}