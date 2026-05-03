using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Shared;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Authentications;

public class UserRoleServiceTests
{
    private readonly Mock<IUserRoleRepository> _userRoleRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly UserRoleService _userRoleService;

    public UserRoleServiceTests()
    {
        _userRoleRepoMock = new Mock<IUserRoleRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _userRoleService = new UserRoleService(
            _userRoleRepoMock.Object,
            _userRepoMock.Object,
            _roleRepoMock.Object
        );
    }

    [Fact]
    public async Task AssignRoleAsync_ValidAssignment_ReturnsSuccess()
    {
        // Arrange
        var dto = new AssignRoleDto { UserId = 1, RoleId = 2 };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        _roleRepoMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Role { Id = 2, Name = "Staff" });
        _userRoleRepoMock.Setup(r => r.UserHasRoleAsync(1, "Staff")).ReturnsAsync(false);
        _userRoleRepoMock
            .Setup(r => r.AddAsync(It.IsAny<UserRole>()))
            .ReturnsAsync((UserRole ur) => ur);

        // Act
        var result = await _userRoleService.AssignRoleAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        _userRoleRepoMock.Verify(r => r.AddAsync(It.IsAny<UserRole>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new AssignRoleDto { UserId = 99, RoleId = 1 };
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _userRoleService.AssignRoleAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task RemoveRoleAsync_ExistingAssignment_RemovesIt()
    {
        // Arrange
        var dto = new AssignRoleDto { UserId = 1, RoleId = 2 };
        var userRole = new UserRole { UserId = 1, RoleId = 2 };
        _userRoleRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(new[] { userRole });
        _userRoleRepoMock.Setup(r => r.DeleteAsync(userRole)).Returns(Task.CompletedTask);

        // Act
        var result = await _userRoleService.RemoveRoleAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        _userRoleRepoMock.Verify(r => r.DeleteAsync(userRole), Times.Once);
    }
}
