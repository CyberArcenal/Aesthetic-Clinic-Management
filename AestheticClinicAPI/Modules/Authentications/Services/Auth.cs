using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AestheticClinicAPI.Modules.Shared;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;

namespace AestheticClinicAPI.Modules.Authentications.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IConfiguration _config;

        public AuthService(
            IUserRepository userRepo,
            IUserRoleRepository userRoleRepo,
            IRoleRepository roleRepo,
            IRefreshTokenRepository refreshTokenRepo,
            IConfiguration config)
        {
            _userRepo = userRepo;
            _userRoleRepo = userRoleRepo;
            _roleRepo = roleRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _config = config;
        }

        private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        private bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

        private string GenerateJwtToken(User user, IEnumerable<string> roles)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key missing")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<RefreshToken> CreateRefreshToken(int userId)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRefreshToken(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            return await _refreshTokenRepo.AddAsync(token);
        }

        private async Task<AuthResponseDto> BuildAuthResponse(User user)
        {
            var roles = await _userRoleRepo.GetUserRolesAsync(user.Id);
            var token = GenerateJwtToken(user, roles);
            var refreshTokenEntity = await CreateRefreshToken(user.Id);
            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Token = token,
                RefreshToken = refreshTokenEntity.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
                Roles = roles
            };
        }

        public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            var existingByUsername = await _userRepo.GetByUsernameAsync(dto.Username);
            if (existingByUsername != null)
                return ServiceResult<AuthResponseDto>.Failure("Username already taken.");

            var existingByEmail = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingByEmail != null)
                return ServiceResult<AuthResponseDto>.Failure("Email already registered.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                FullName = dto.FullName,
                IsActive = true
            };
            var created = await _userRepo.AddAsync(user);

            // Assign default role "Client" (create role if missing)
            var clientRole = await _roleRepo.GetByNameAsync("Client");
            if (clientRole == null)
            {
                clientRole = await _roleRepo.AddAsync(new Role { Name = "Client", Description = "Default client role" });
            }
            await _userRoleRepo.AddAsync(new UserRole { UserId = created.Id, RoleId = clientRole.Id });

            var response = await BuildAuthResponse(created);
            return ServiceResult<AuthResponseDto>.Success(response);
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetByUsernameAsync(dto.UsernameOrEmail) ??
                       await _userRepo.GetByEmailAsync(dto.UsernameOrEmail);
            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                return ServiceResult<AuthResponseDto>.Failure("Invalid username/email or password.");

            if (!user.IsActive)
                return ServiceResult<AuthResponseDto>.Failure("Account is disabled.");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            // Revoke old refresh tokens (optional)
            await _refreshTokenRepo.RevokeAllForUserAsync(user.Id);

            var response = await BuildAuthResponse(user);
            return ServiceResult<AuthResponseDto>.Success(response);
        }

        public async Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
            if (tokenEntity == null || tokenEntity.ExpiryDate < DateTime.UtcNow || tokenEntity.IsRevoked)
                return ServiceResult<AuthResponseDto>.Failure("Invalid or expired refresh token.");

            var user = await _userRepo.GetByIdAsync(tokenEntity.UserId);
            if (user == null || !user.IsActive)
                return ServiceResult<AuthResponseDto>.Failure("User not found or inactive.");

            // Revoke the used refresh token and create a new one
            tokenEntity.IsRevoked = true;
            await _refreshTokenRepo.UpdateAsync(tokenEntity);

            var response = await BuildAuthResponse(user);
            return ServiceResult<AuthResponseDto>.Success(response);
        }

        public async Task<ServiceResult<bool>> LogoutAsync(int userId)
        {
            await _refreshTokenRepo.RevokeAllForUserAsync(userId);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<AuthResponseDto>> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return ServiceResult<AuthResponseDto>.Failure("User not found.");
            var roles = await _userRoleRepo.GetUserRolesAsync(user.Id);
            var token = GenerateJwtToken(user, roles);
            // Return new token for the current user (optional)
            return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Token = token,
                RefreshToken = "",  // not needed for current user
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
                Roles = roles
            });
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found.");
            if (!VerifyPassword(currentPassword, user.PasswordHash))
                return ServiceResult<bool>.Failure("Current password is incorrect.");

            user.PasswordHash = HashPassword(newPassword);
            await _userRepo.UpdateAsync(user);
            // Optionally revoke all refresh tokens after password change
            await _refreshTokenRepo.RevokeAllForUserAsync(userId);
            return ServiceResult<bool>.Success(true);
        }
    }
}