using Moq;
using Xunit;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using System.Linq.Expressions;
using BCrypt.Net;

namespace AestheticClinicAPI.Tests.UnitTests.Authentications;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserRoleRepository> _userRoleRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _userRoleRepoMock = new Mock<IUserRoleRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _userService = new UserService(_userRepoMock.Object, _userRoleRepoMock.Object, _roleRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDtoWithRoles()
    {
        // Arrange
        var user = new User { Id = 1, Username = "john", Email = "john@example.com", IsActive = true };
        var roles = new[] { "Admin", "Staff" };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRoleRepoMock.Setup(r => r.GetUserRolesAsync(1)).ReturnsAsync(roles);

        // Act
        var result = await _userService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("john", result.Data?.Username);
        Assert.Equal(roles, result.Data?.Roles);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsFailure()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "Pass123",
            FullName = "New User",
            IsActive = true,
            Roles = new[] { "Client" }
        };
        var createdUser = new User { Id = 1, Username = dto.Username, Email = dto.Email };
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _roleRepoMock.Setup(r => r.GetByNameAsync("Client")).ReturnsAsync(new Role { Id = 1, Name = "Client" });
        _userRoleRepoMock.Setup(r => r.AddAsync(It.IsAny<UserRole>())).ReturnsAsync((UserRole ur) => ur);
        _userRoleRepoMock.Setup(r => r.GetUserRolesAsync(1)).ReturnsAsync(new[] { "Client" });

        // Act
        var result = await _userService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Username, result.Data?.Username);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsername_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateUserDto { Username = "existing", Email = "new@example.com", Password = "Pass" };
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username))
            .ReturnsAsync(new User { Id = 1, Username = "existing" });

        // Act
        var result = await _userService.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Username already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task ActivateAsync_ValidUser_TogglesActive()
    {
        // Arrange
        var user = new User { Id = 1, IsActive = false };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.ActivateAsync(1, true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.IsActive);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_SoftDeletesUser()
    {
        // Arrange
        var user = new User { Id = 1 };
        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.DeleteAsync(user)).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepoMock.Verify(r => r.DeleteAsync(user), Times.Once);
    }
}