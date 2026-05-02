using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Services;

namespace AestheticClinicAPI.Modules.Authentications.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<AuthResponseDto>.Ok(result.Data!, "Registration successful."));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.IsSuccess)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<AuthResponseDto>.Ok(result.Data!, "Login successful."));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
            if (!result.IsSuccess)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<AuthResponseDto>.Ok(result.Data!, "Token refreshed."));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _authService.LogoutAsync(userId);
            return Ok(ApiResponse<bool>.Ok(true, "Logged out."));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _authService.GetCurrentUserAsync(userId);
            if (!result.IsSuccess)
                return NotFound(ApiResponse<AuthResponseDto>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<AuthResponseDto>.Ok(result.Data!));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage!));
            return Ok(ApiResponse<bool>.Ok(true, "Password changed."));
        }
    }
}