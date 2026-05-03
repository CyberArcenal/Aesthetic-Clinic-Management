using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Shared;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Authentications;

public class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly RefreshTokenService _refreshTokenService;

    public RefreshTokenServiceTests()
    {
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _refreshTokenService = new RefreshTokenService(_refreshTokenRepoMock.Object);
    }

    [Fact]
    public async Task CreateTokenAsync_ReturnsNewToken()
    {
        // Arrange
        var userId = 1;
        RefreshToken capturedToken = null;
        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(rt => capturedToken = rt)
            .ReturnsAsync((RefreshToken rt) => rt);

        // Act
        var result = await _refreshTokenService.CreateTokenAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedToken);
        Assert.Equal(userId, capturedToken.UserId);
        Assert.False(capturedToken.IsRevoked);
        Assert.True(capturedToken.ExpiryDate > DateTime.UtcNow);
        Assert.NotEmpty(capturedToken.Token);
    }

    [Fact]
    public async Task RevokeTokenAsync_ExistingToken_RevokesIt()
    {
        // Arrange
        var token = "some-token";
        var tokenEntity = new RefreshToken
        {
            Id = 1,
            Token = token,
            IsRevoked = false,
        };
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(token)).ReturnsAsync(tokenEntity);
        _refreshTokenRepoMock.Setup(r => r.UpdateAsync(tokenEntity)).Returns(Task.CompletedTask);

        // Act
        var result = await _refreshTokenService.RevokeTokenAsync(token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(tokenEntity.IsRevoked);
        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(tokenEntity), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_NonExistingToken_ReturnsFailure()
    {
        // Arrange
        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("invalid"))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _refreshTokenService.RevokeTokenAsync("invalid");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Token not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task RevokeAllForUserAsync_CallsRepository()
    {
        // Arrange
        var userId = 1;
        _refreshTokenRepoMock
            .Setup(r => r.RevokeAllForUserAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _refreshTokenService.RevokeAllForUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _refreshTokenRepoMock.Verify(r => r.RevokeAllForUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CleanExpiredAsync_CallsRepository()
    {
        // Arrange
        _refreshTokenRepoMock.Setup(r => r.CleanExpiredTokensAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _refreshTokenService.CleanExpiredAsync();

        // Assert
        Assert.True(result.IsSuccess);
        _refreshTokenRepoMock.Verify(r => r.CleanExpiredTokensAsync(), Times.Once);
    }
}
