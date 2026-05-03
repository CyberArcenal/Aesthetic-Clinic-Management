using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Treatments.DTOs;
using AestheticClinicAPI.Modules.Treatments.Services;

namespace AestheticClinicAPI.Modules.Treatments.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TreatmentsController : ControllerBase
    {
        private readonly ITreatmentService _treatmentService;

        public TreatmentsController(ITreatmentService treatmentService)
        {
            _treatmentService = treatmentService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<TreatmentResponseDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _treatmentService.GetPaginatedAsync(page, pageSize, search);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<TreatmentResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<TreatmentResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TreatmentResponseDto>>> GetById(int id)
        {
            var result = await _treatmentService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<TreatmentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<TreatmentResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TreatmentResponseDto>>> Create([FromBody] CreateTreatmentDto dto)
        {
            var result = await _treatmentService.CreateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<TreatmentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<TreatmentResponseDto>.Ok(result.Data!, "Treatment created."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TreatmentResponseDto>>> Update(int id, [FromBody] UpdateTreatmentDto dto)
        {
            var result = await _treatmentService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<TreatmentResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<TreatmentResponseDto>.Ok(result.Data!, "Treatment updated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _treatmentService.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Treatment deleted."));
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(int id)
        {
            var result = await _treatmentService.ToggleActiveAsync(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Treatment active status toggled."));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TreatmentResponseDto>>>> GetActive()
        {
            var result = await _treatmentService.GetActiveAsync();
            return Ok(ApiResponse<IEnumerable<TreatmentResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TreatmentResponseDto>>>> GetByCategory(string category)
        {
            var result = await _treatmentService.GetByCategoryAsync(category);
            return Ok(ApiResponse<IEnumerable<TreatmentResponseDto>>.Ok(result.Data!));
        }
    }
}