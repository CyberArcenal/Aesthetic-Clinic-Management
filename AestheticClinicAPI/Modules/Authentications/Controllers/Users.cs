using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Services;

namespace AestheticClinicAPI.Modules.Authentications.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<UserResponseDto>>>> GetAll(
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] string? search = null)
        {
            var result = await _userService.GetPaginatedAsync(page, pageSize, search);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<PaginatedResult<UserResponseDto>>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<PaginatedResult<UserResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<UserResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<UserResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> Create([FromBody] CreateUserDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<UserResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<UserResponseDto>.Ok(result.Data!, "User created."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<UserResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<UserResponseDto>.Ok(result.Data!, "User updated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "User deleted."));
        }

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> Activate(int id, [FromQuery] bool isActive = true)
        {
            var result = await _userService.ActivateAsync(id, isActive);
            if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, isActive ? "User activated." : "User deactivated."));
        }
    }
}