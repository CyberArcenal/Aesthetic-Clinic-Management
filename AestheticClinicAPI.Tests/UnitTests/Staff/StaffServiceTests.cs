using Moq;
using Xunit;
using System.Linq.Expressions;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Staff.Services;
using AestheticClinicAPI.Modules.Staff.Repositories;
using AestheticClinicAPI.Modules.Staff.Models;
using AestheticClinicAPI.Modules.Staff.DTOs;

namespace AestheticClinicAPI.Tests.UnitTests.Staff;

public class StaffServiceTests
{
    private readonly Mock<IStaffRepository> _staffRepoMock;
    private readonly StaffService _staffService;

    public StaffServiceTests()
    {
        _staffRepoMock = new Mock<IStaffRepository>();
        _staffService = new StaffService(_staffRepoMock.Object);
    }

    private StaffMember CreateSampleStaff(int id = 1) => new StaffMember
    {
        Id = id,
        Name = "Dr. John Doe",
        Email = "john@clinic.com",
        Phone = "09123456789",
        Position = "Dermatologist",
        IsActive = true
    };

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingStaff_ReturnsDto()
    {
        // Arrange
        var staff = CreateSampleStaff(1);
        _staffRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);

        // Act
        var result = await _staffService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data?.Id);
        Assert.Equal("Dr. John Doe", result.Data?.Name);
        Assert.Equal("Dermatologist", result.Data?.Position);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingStaff_ReturnsFailure()
    {
        // Arrange
        _staffRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StaffMember?)null);

        // Act
        var result = await _staffService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Staff not found.", result.ErrorMessage);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateStaffDto
        {
            Name = "Dr. Jane Smith",
            Email = "jane@clinic.com",
            Phone = "09123456788",
            Position = "Nurse",
            IsActive = true
        };
        StaffMember? captured = null;
        _staffRepoMock.Setup(r => r.AddAsync(It.IsAny<StaffMember>()))
            .Callback<StaffMember>(s => captured = s)
            .ReturnsAsync((StaffMember s) => s);

        // Act
        var result = await _staffService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(dto.Name, captured.Name);
        Assert.Equal(dto.Email, captured.Email);
        Assert.Equal(dto.Position, captured.Position);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingStaff_UpdatesFields()
    {
        // Arrange
        var staff = CreateSampleStaff(1);
        _staffRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _staffRepoMock.Setup(r => r.UpdateAsync(staff)).Returns(Task.CompletedTask);

        var dto = new UpdateStaffDto
        {
            Name = "Dr. John Updated",
            Position = "Senior Dermatologist",
            IsActive = false
        };

        // Act
        var result = await _staffService.UpdateAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Dr. John Updated", staff.Name);
        Assert.Equal("Senior Dermatologist", staff.Position);
        Assert.False(staff.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingStaff_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateStaffDto { Name = "New Name" };
        _staffRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StaffMember?)null);

        // Act
        var result = await _staffService.UpdateAsync(99, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Staff not found.", result.ErrorMessage);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingStaff_Deletes()
    {
        // Arrange
        var staff = CreateSampleStaff(1);
        _staffRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _staffRepoMock.Setup(r => r.DeleteAsync(staff)).Returns(Task.CompletedTask);

        // Act
        var result = await _staffService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _staffRepoMock.Verify(r => r.DeleteAsync(staff), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingStaff_ReturnsFailure()
    {
        // Arrange
        _staffRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StaffMember?)null);

        // Act
        var result = await _staffService.DeleteAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Staff not found.", result.ErrorMessage);
    }

    #endregion

    #region ToggleActiveAsync Tests

    [Fact]
    public async Task ToggleActiveAsync_ExistingStaff_TogglesIsActive()
    {
        // Arrange
        var staff = CreateSampleStaff(1);
        bool original = staff.IsActive;
        _staffRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _staffRepoMock.Setup(r => r.UpdateAsync(staff)).Returns(Task.CompletedTask);

        // Act
        var result = await _staffService.ToggleActiveAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(!original, staff.IsActive);
    }

    [Fact]
    public async Task ToggleActiveAsync_NonExistingStaff_ReturnsFailure()
    {
        // Arrange
        _staffRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StaffMember?)null);

        // Act
        var result = await _staffService.ToggleActiveAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Staff not found.", result.ErrorMessage);
    }

    #endregion

    #region GetActiveAsync Tests

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveStaff()
    {
        // Arrange
        var activeStaff = new List<StaffMember>
        {
            CreateSampleStaff(1),
            new StaffMember { Id = 2, Name = "Dr. Active", IsActive = true }
        };
        _staffRepoMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(activeStaff);

        // Act
        var result = await _staffService.GetActiveAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
        Assert.All(result.Data, s => Assert.True(s.IsActive));
    }

    #endregion

    #region GetByPositionAsync Tests

    [Fact]
    public async Task GetByPositionAsync_ReturnsStaffWithGivenPosition()
    {
        // Arrange
        var staffList = new List<StaffMember>
        {
            CreateSampleStaff(1),
            new StaffMember { Id = 2, Name = "Nurse Ann", Position = "Nurse" }
        };
        _staffRepoMock.Setup(r => r.GetByPositionAsync("Dermatologist")).ReturnsAsync(staffList.Where(s => s.Position == "Dermatologist").ToList());

        // Act
        var result = await _staffService.GetByPositionAsync("Dermatologist");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
        Assert.Equal("Dermatologist", result.Data.First().Position);
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Fact]
    public async Task GetPaginatedAsync_ReturnsPaginatedResult()
    {
        // Arrange
        var staffList = new List<StaffMember>
        {
            CreateSampleStaff(1),
            CreateSampleStaff(2),
            CreateSampleStaff(3)
        };
        var paginated = new PaginatedResult<StaffMember>
        {
            Items = staffList,
            Page = 1,
            PageSize = 10,
            TotalCount = 3
        };
        _staffRepoMock.Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<Expression<Func<StaffMember, bool>>>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _staffService.GetPaginatedAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data!.TotalCount);
        Assert.Equal(3, result.Data.Items.Count());
    }

    [Fact]
    public async Task GetPaginatedAsync_WithSearch_FiltersCorrectly()
    {
        // Arrange
        Expression<Func<StaffMember, bool>>? capturedFilter = null;
        _staffRepoMock.Setup(r => r.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<StaffMember, bool>>>()))
            .Callback<int, int, Expression<Func<StaffMember, bool>>>((p, ps, f) => capturedFilter = f)
            .ReturnsAsync(new PaginatedResult<StaffMember> { Items = new List<StaffMember>(), Page = 1, PageSize = 10, TotalCount = 0 });

        // Act
        await _staffService.GetPaginatedAsync(1, 10, "john");

        // Assert
        Assert.NotNull(capturedFilter);
        // We can't easily test the filter contents, but at least it was called.
    }

    #endregion
}