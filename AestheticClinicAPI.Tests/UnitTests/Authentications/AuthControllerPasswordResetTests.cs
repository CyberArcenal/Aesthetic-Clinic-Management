using AestheticClinicAPI.Modules.Authentications.Controllers;
using AestheticClinicAPI.Modules.Authentications.Controllers.v1;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AestheticClinicAPI.Tests.IntegrationTests.Authentications;

public class AuthControllerPasswordResetTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerPasswordResetTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_ReturnsOk()
    {
        // Arrange
        var dto = new ForgotPasswordDto { Email = "user@example.com" };
        _authServiceMock
            .Setup(s => s.ForgotPasswordAsync(dto.Email))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        // Act
        var result = await _controller.ForgotPassword(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task ResetPassword_ValidToken_ReturnsOk()
    {
        // Arrange
        var dto = new ResetPasswordDto { Token = "valid", NewPassword = "NewPass123!" };
        _authServiceMock
            .Setup(s => s.ResetPasswordAsync(dto.Token, dto.NewPassword))
            .ReturnsAsync(ServiceResult<bool>.Success(true));

        // Act
        var result = await _controller.ResetPassword(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }
}
