using Moq;
using Xunit;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace AestheticClinicAPI.Tests.UnitTests.Authentications;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserRoleRepository> _userRoleRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _userRoleRepoMock = new Mock<IUserRoleRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _configMock = new Mock<IConfiguration>();

        // Setup JWT configuration
        _configMock.Setup(c => c["Jwt:Key"]).Returns("test-key-that-is-at-least-32-characters-long");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

        _authService = new AuthService(
            _userRepoMock.Object,
            _userRoleRepoMock.Object,
            _roleRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _configMock.Object);

        var jwtSettingsMock = new Mock<IConfigurationSection>();
        jwtSettingsMock.Setup(x => x["Key"]).Returns("test-key-that-is-at-least-32-characters-long");
        jwtSettingsMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
        jwtSettingsMock.Setup(x => x["Audience"]).Returns("TestAudience");
        jwtSettingsMock.Setup(x => x["ExpiryMinutes"]).Returns("60");

        _configMock.Setup(c => c.GetSection("Jwt")).Returns(jwtSettingsMock.Object);
        _configMock.Setup(c => c["Jwt:Key"]).Returns("test-key-that-is-at-least-32-characters-long");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

        // Also ensure these top-level indexers work for direct access
        _configMock.Setup(c => c["Jwt:Key"]).Returns("test-key-that-is-at-least-32-characters-long");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsSuccess()
    {
        // Arrange
        var dto = new RegisterDto
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "Password123",
            FullName = "New User"
        };

        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _roleRepoMock.Setup(r => r.GetByNameAsync("Client"))
            .ReturnsAsync(new Role { Id = 1, Name = "Client" });
        _userRoleRepoMock.Setup(r => r.AddAsync(It.IsAny<UserRole>()))
            .ReturnsAsync((UserRole ur) => ur);
        _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);
        _userRoleRepoMock.Setup(r => r.GetUserRolesAsync(It.IsAny<int>()))
            .ReturnsAsync(new[] { "Client" });

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Username, result.Data.Username);
        Assert.Equal(dto.Email, result.Data.Email);
        Assert.NotNull(result.Data.Token);
        Assert.NotEmpty(result.Data.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_UsernameExists_ReturnsFailure()
    {
        // Arrange
        var dto = new RegisterDto { Username = "existing", Email = "new@example.com", Password = "Pass123" };
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username))
            .ReturnsAsync(new User { Id = 1, Username = "existing" });

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Username already taken.", result.ErrorMessage);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_EmailExists_ReturnsFailure()
    {
        // Arrange
        var dto = new RegisterDto { Username = "newuser", Email = "existing@example.com", Password = "Pass123" };
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new User { Id = 1, Email = "existing@example.com" });

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email already registered.", result.ErrorMessage);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var password = "Password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            IsActive = true
        };
        var dto = new LoginDto { UsernameOrEmail = "testuser", Password = password };

        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.UsernameOrEmail)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _refreshTokenRepoMock.Setup(r => r.RevokeAllForUserAsync(user.Id)).Returns(Task.CompletedTask);
        _userRoleRepoMock.Setup(r => r.GetUserRolesAsync(user.Id)).ReturnsAsync(new[] { "Client" });
        _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Username, result.Data.Username);
        Assert.NotNull(result.Data.Token);
        Assert.NotEmpty(result.Data.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass123"),
            IsActive = true
        };
        var dto = new LoginDto { UsernameOrEmail = "testuser", Password = "WrongPass" };
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.UsernameOrEmail)).ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid username/email or password.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new LoginDto { UsernameOrEmail = "nonexistent", Password = "pass" };
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.UsernameOrEmail)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.UsernameOrEmail)).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid username/email or password.", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "inactive",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"),
            IsActive = false
        };
        var dto = new LoginDto { UsernameOrEmail = "inactive", Password = "Pass123" };
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.UsernameOrEmail)).ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account is disabled.", result.ErrorMessage);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var tokenEntity = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(refreshToken)).ReturnsAsync(tokenEntity);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _refreshTokenRepoMock.Setup(r => r.UpdateAsync(tokenEntity)).Returns(Task.CompletedTask);
        _userRoleRepoMock.Setup(r => r.GetUserRolesAsync(user.Id)).ReturnsAsync(new[] { "Client" });
        _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Token);
        Assert.NotEmpty(result.Data.RefreshToken);
        Assert.True(tokenEntity.IsRevoked); // old token should be revoked
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "expired-token";
        var tokenEntity = new RefreshToken
        {
            Id = 1,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(refreshToken)).ReturnsAsync(tokenEntity);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid or expired refresh token.", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "revoked-token";
        var tokenEntity = new RefreshToken
        {
            Id = 1,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(refreshToken)).ReturnsAsync(tokenEntity);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid or expired refresh token.", result.ErrorMessage);
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_RevokesAllTokens_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        _refreshTokenRepoMock.Setup(r => r.RevokeAllForUserAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LogoutAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _refreshTokenRepoMock.Verify(r => r.RevokeAllForUserAsync(userId), Times.Once);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_ValidCredentials_ChangesPasswordAndRevokesTokens()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "OldPass123";
        var newPassword = "NewPass456";
        var user = new User
        {
            Id = userId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(currentPassword)
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _refreshTokenRepoMock.Setup(r => r.RevokeAllForUserAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.RevokeAllForUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_IncorrectCurrentPassword_ReturnsFailure()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass")
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, "WrongPass", "NewPass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Current password is incorrect.", result.ErrorMessage);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.ChangePasswordAsync(99, "any", "any");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    #endregion

    #region GetCurrentUserAsync Tests

    [Fact]
    public async Task GetCurrentUserAsync_ExistingUser_ReturnsUserWithNewToken()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FullName = "Test User"
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRoleRepoMock.Setup(r => r.GetUserRolesAsync(userId)).ReturnsAsync(new[] { "Client" });

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user.Username, result.Data?.Username);
        Assert.NotNull(result.Data?.Token);
        Assert.Empty(result.Data?.RefreshToken ?? ""); // no refresh token returned
    }

    [Fact]
    public async Task GetCurrentUserAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.GetCurrentUserAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    #endregion
}