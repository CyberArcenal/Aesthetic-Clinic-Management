using Moq;
using Xunit;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Models;
using System.Linq.Expressions;
using BCrypt.Net;

namespace AestheticClinicAPI.Tests.UnitTests.Authentications;

public class UserServicePasswordResetTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserRoleRepository> _userRoleRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IPasswordResetTokenRepository> _tokenRepoMock;
    private readonly UserService _userService;

    public UserServicePasswordResetTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _userRoleRepoMock = new Mock<IUserRoleRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _tokenRepoMock = new Mock<IPasswordResetTokenRepository>();
        _userService = new UserService(
            _userRepoMock.Object,
            _userRoleRepoMock.Object,
            _roleRepoMock.Object,
            _tokenRepoMock.Object);
    }

    #region GeneratePasswordResetTokenAsync Tests

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_ValidUser_ReturnsToken()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Username = "testuser" };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _tokenRepoMock.Setup(r => r.RevokeAllForUserAsync(userId)).Returns(Task.CompletedTask);
        _tokenRepoMock.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>()))
            .ReturnsAsync((PasswordResetToken t) => t);

        // Act
        var result = await _userService.GeneratePasswordResetTokenAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
        _tokenRepoMock.Verify(r => r.RevokeAllForUserAsync(userId), Times.Once);
        _tokenRepoMock.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GeneratePasswordResetTokenAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.ErrorMessage);
        _tokenRepoMock.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_UpdatesPasswordAndMarksTokenUsed()
    {
        // Arrange
        var token = "valid-token";
        var newPassword = "NewSecurePass123!";
        var resetTokenEntity = new PasswordResetToken
        {
            Id = 1,
            UserId = 1,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };
        var user = new User { Id = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("old") };

        _tokenRepoMock.Setup(r => r.GetByTokenAsync(token)).ReturnsAsync(resetTokenEntity);
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _tokenRepoMock.Setup(r => r.UpdateAsync(resetTokenEntity)).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.ResetPasswordAsync(token, newPassword);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash));
        Assert.True(resetTokenEntity.IsUsed);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _tokenRepoMock.Verify(r => r.UpdateAsync(resetTokenEntity), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ReturnsFailure()
    {
        // Arrange
        _tokenRepoMock.Setup(r => r.GetByTokenAsync("invalid")).ReturnsAsync((PasswordResetToken?)null);

        // Act
        var result = await _userService.ResetPasswordAsync("invalid", "newpass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid or expired reset token.", result.ErrorMessage);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var token = "expired-token";
        var resetTokenEntity = new PasswordResetToken
        {
            Id = 1,
            UserId = 1,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddHours(-1), // expired
            IsUsed = false
        };
        _tokenRepoMock.Setup(r => r.GetByTokenAsync(token)).ReturnsAsync((PasswordResetToken?)null); // because repository filters by expiry

        // Act
        var result = await _userService.ResetPasswordAsync(token, "newpass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid or expired reset token.", result.ErrorMessage);
    }

    [Fact]
    public async Task ResetPasswordAsync_TokenUserNotFound_ReturnsFailure()
    {
        // Arrange
        var token = "valid-token";
        var resetTokenEntity = new PasswordResetToken
        {
            Id = 1,
            UserId = 99,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };
        _tokenRepoMock.Setup(r => r.GetByTokenAsync(token)).ReturnsAsync(resetTokenEntity);
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.ResetPasswordAsync(token, "newpass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.ErrorMessage);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _tokenRepoMock.Verify(r => r.UpdateAsync(resetTokenEntity), Times.Never);
    }

    #endregion
}