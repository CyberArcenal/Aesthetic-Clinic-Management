using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Services;

namespace AestheticClinicAPI.Modules.Authentications.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/v1/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;

        public RolesController(IRoleService roleService, IUserRoleService userRoleService)
        {
            _roleService = roleService;
            _userRoleService = userRoleService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoleResponseDto>>>> GetAll()
        {
            var result = await _roleService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<RoleResponseDto>>.Ok(result.Data!));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoleResponseDto>>> GetById(int id)
        {
            var result = await _roleService.GetByIdAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<RoleResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<RoleResponseDto>.Ok(result.Data!));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<RoleResponseDto>>> Create([FromBody] CreateRoleDto dto)
        {
            var result = await _roleService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<RoleResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<RoleResponseDto>.Ok(result.Data!, "Role created."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RoleResponseDto>>> Update(int id, [FromBody] UpdateRoleDto dto)
        {
            var result = await _roleService.UpdateAsync(id, dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<RoleResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<RoleResponseDto>.Ok(result.Data!, "Role updated."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _roleService.DeleteAsync(id);
            if (!result.IsSuccess) return NotFound(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Role deleted."));
        }

        [HttpPost("assign")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignRole([FromBody] AssignRoleDto dto)
        {
            var result = await _userRoleService.AssignRoleAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Role assigned."));
        }

        [HttpPost("remove")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveRole([FromBody] AssignRoleDto dto)
        {
            var result = await _userRoleService.RemoveRoleAsync(dto);
            if (!result.IsSuccess) return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Role removed."));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetUserRoles(int userId)
        {
            var result = await _userRoleService.GetUserRolesAsync(userId);
            return Ok(ApiResponse<IEnumerable<string>>.Ok(result.Data!));
        }
    }
}