using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Modules.Authentications.Repositories;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Shared;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Authentications;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _roleRepoMock = new Mock<IRoleRepository>();
        _roleService = new RoleService(_roleRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRole_ReturnsDto()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            Description = "Full access",
        };
        _roleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(role);

        // Act
        var result = await _roleService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Admin", result.Data?.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsFailure()
    {
        // Arrange
        _roleRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Role?)null);

        // Act
        var result = await _roleService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Role not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_NewRole_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateRoleDto { Name = "Manager", Description = "Manager role" };
        _roleRepoMock.Setup(r => r.GetByNameAsync(dto.Name)).ReturnsAsync((Role?)null);
        _roleRepoMock.Setup(r => r.AddAsync(It.IsAny<Role>())).ReturnsAsync((Role r) => r);

        // Act
        var result = await _roleService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Manager", result.Data?.Name);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateRoleDto { Name = "Admin" };
        _roleRepoMock
            .Setup(r => r.GetByNameAsync("Admin"))
            .ReturnsAsync(new Role { Id = 1, Name = "Admin" });

        // Act
        var result = await _roleService.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Role name already exists.", result.ErrorMessage);
    }
}
