using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Staff.DTOs;
using AestheticClinicAPI.Modules.Staff.Services;

namespace AestheticClinicAPI.Modules.Staff.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<StaffResponseDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _staffService.GetPaginatedAsync(page, pageSize, search);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<StaffResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<StaffResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StaffResponseDto>>> GetById(int id)
        {
            var result = await _staffService.GetByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<StaffResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<StaffResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<StaffResponseDto>>> Create([FromBody] CreateStaffDto dto)
        {
            var result = await _staffService.CreateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<StaffResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<StaffResponseDto>.Ok(result.Data!, "Staff created."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<StaffResponseDto>>> Update(int id, [FromBody] UpdateStaffDto dto)
        {
            var result = await _staffService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<StaffResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<StaffResponseDto>.Ok(result.Data!, "Staff updated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _staffService.DeleteAsync(id);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Staff deleted."));
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(int id)
        {
            var result = await _staffService.ToggleActiveAsync(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Staff active status toggled."));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StaffResponseDto>>>> GetActive()
        {
            var result = await _staffService.GetActiveAsync();
            return Ok(ApiResponse<IEnumerable<StaffResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("position/{position}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StaffResponseDto>>>> GetByPosition(string position)
        {
            var result = await _staffService.GetByPositionAsync(position);
            return Ok(ApiResponse<IEnumerable<StaffResponseDto>>.Ok(result.Data!));
        }
    }
}