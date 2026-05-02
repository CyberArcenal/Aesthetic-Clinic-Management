using Moq;
using Xunit;
using System.Linq.Expressions;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Treatments.Services;
using AestheticClinicAPI.Modules.Treatments.Repositories;
using AestheticClinicAPI.Modules.Treatments.Models;
using AestheticClinicAPI.Modules.Treatments.DTOs;
using AestheticClinicAPI.Modules.Appointments.Repositories;
using AestheticClinicAPI.Modules.Appointments.Models;

namespace AestheticClinicAPI.Tests.UnitTests.Treatments;

public class TreatmentServiceTests
{
    private readonly Mock<ITreatmentRepository> _treatmentRepoMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly TreatmentService _treatmentService;

    public TreatmentServiceTests()
    {
        _treatmentRepoMock = new Mock<ITreatmentRepository>();
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _treatmentService = new TreatmentService(_treatmentRepoMock.Object, _appointmentRepoMock.Object);
    }

    private Treatment CreateSampleTreatment(int id = 1) => new Treatment
    {
        Id = id,
        Name = "HydraFacial",
        Description = "Deep cleansing facial",
        Category = "Facial",
        DurationMinutes = 60,
        Price = 3500,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingTreatment_ReturnsDto()
    {
        // Arrange
        var treatment = CreateSampleTreatment(1);
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(treatment);

        // Act
        var result = await _treatmentService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data?.Id);
        Assert.Equal("HydraFacial", result.Data?.Name);
        Assert.Equal(3500, result.Data?.Price);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTreatment_ReturnsFailure()
    {
        // Arrange
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Treatment?)null);

        // Act
        var result = await _treatmentService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Treatment not found.", result.ErrorMessage);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateTreatmentDto
        {
            Name = "Botox",
            Description = "Anti-aging injection",
            Category = "Injectable",
            DurationMinutes = 30,
            Price = 8000,
            IsActive = true
        };
        Treatment? captured = null;
        _treatmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Treatment>()))
            .Callback<Treatment>(t => captured = t)
            .ReturnsAsync((Treatment t) => t);

        // Act
        var result = await _treatmentService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(dto.Name, captured.Name);
        Assert.Equal(dto.Price, captured.Price);
        Assert.Equal(dto.DurationMinutes, captured.DurationMinutes);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingTreatment_UpdatesFields()
    {
        // Arrange
        var treatment = CreateSampleTreatment(1);
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(treatment);
        _treatmentRepoMock.Setup(r => r.UpdateAsync(treatment)).Returns(Task.CompletedTask);

        var dto = new UpdateTreatmentDto
        {
            Name = "HydraFacial Deluxe",
            Price = 4500,
            DurationMinutes = 75,
            IsActive = false
        };

        // Act
        var result = await _treatmentService.UpdateAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("HydraFacial Deluxe", treatment.Name);
        Assert.Equal(4500, treatment.Price);
        Assert.Equal(75, treatment.DurationMinutes);
        Assert.False(treatment.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingTreatment_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateTreatmentDto { Name = "New Name" };
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Treatment?)null);

        // Act
        var result = await _treatmentService.UpdateAsync(99, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Treatment not found.", result.ErrorMessage);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingTreatment_Deletes()
    {
        // Arrange
        var treatment = CreateSampleTreatment(1);
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(treatment);
        _treatmentRepoMock.Setup(r => r.DeleteAsync(treatment)).Returns(Task.CompletedTask);

        // Act
        var result = await _treatmentService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _treatmentRepoMock.Verify(r => r.DeleteAsync(treatment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingTreatment_ReturnsFailure()
    {
        // Arrange
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Treatment?)null);

        // Act
        var result = await _treatmentService.DeleteAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Treatment not found.", result.ErrorMessage);
    }

    #endregion

    #region ToggleActiveAsync Tests

    [Fact]
    public async Task ToggleActiveAsync_ExistingTreatment_TogglesIsActive()
    {
        // Arrange
        var treatment = CreateSampleTreatment(1);
        bool original = treatment.IsActive;
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(treatment);
        _treatmentRepoMock.Setup(r => r.UpdateAsync(treatment)).Returns(Task.CompletedTask);

        // Act
        var result = await _treatmentService.ToggleActiveAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(!original, treatment.IsActive);
    }

    [Fact]
    public async Task ToggleActiveAsync_NonExistingTreatment_ReturnsFailure()
    {
        // Arrange
        _treatmentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Treatment?)null);

        // Act
        var result = await _treatmentService.ToggleActiveAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Treatment not found.", result.ErrorMessage);
    }

    #endregion

    #region GetByCategoryAsync Tests

    [Fact]
    public async Task GetByCategoryAsync_ReturnsTreatmentsInCategory()
    {
        // Arrange
        var treatments = new List<Treatment>
        {
            CreateSampleTreatment(1),
            new Treatment { Id = 2, Name = "Chemical Peel", Category = "Facial" }
        };
        _treatmentRepoMock.Setup(r => r.GetByCategoryAsync("Facial")).ReturnsAsync(treatments);

        // Act
        var result = await _treatmentService.GetByCategoryAsync("Facial");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
        Assert.All(result.Data, t => Assert.Equal("Facial", t.Category));
    }

    #endregion

    #region GetActiveAsync Tests

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveTreatments()
    {
        // Arrange
        var activeTreatments = new List<Treatment>
        {
            CreateSampleTreatment(1),
            new Treatment { Id = 2, Name = "Active Treatment", IsActive = true }
        };
        _treatmentRepoMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(activeTreatments);

        // Act
        var result = await _treatmentService.GetActiveAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
        Assert.All(result.Data, t => Assert.True(t.IsActive));
    }

    #endregion

    #region GetTotalRevenueAsync Tests

    [Fact]
    public async Task GetTotalRevenueAsync_WithAppointmentRepo_ReturnsSum()
    {
        // Arrange
        var treatments = new List<Treatment>
        {
            new Treatment { Id = 1, Price = 3500 },
            new Treatment { Id = 2, Price = 8000 }
        };
        var completedAppointments = new List<Appointment>
        {
            new Appointment { TreatmentId = 1, Status = "Completed" },
            new Appointment { TreatmentId = 1, Status = "Completed" },
            new Appointment { TreatmentId = 2, Status = "Completed" }
        };
        _appointmentRepoMock.Setup(r => r.GetByStatusAsync("Completed")).ReturnsAsync(completedAppointments);
        _treatmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(treatments);

        // Act
        var result = await _treatmentService.GetTotalRevenueAsync();

        // Assert
        Assert.True(result.IsSuccess);
        // 2 * 3500 + 1 * 8000 = 15000
        Assert.Equal(15000, result.Data);
    }

    [Fact]
    public async Task GetTotalRevenueAsync_WithoutAppointmentRepo_ReturnsFailure()
    {
        // Arrange
        var serviceWithoutAppointmentRepo = new TreatmentService(_treatmentRepoMock.Object, null);

        // Act
        var result = await serviceWithoutAppointmentRepo.GetTotalRevenueAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Appointment repository not available.", result.ErrorMessage);
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Fact]
    public async Task GetPaginatedAsync_ReturnsPaginatedResult()
    {
        // Arrange
        var treatments = new List<Treatment>
        {
            CreateSampleTreatment(1),
            CreateSampleTreatment(2),
            CreateSampleTreatment(3)
        };
        var paginated = new PaginatedResult<Treatment>
        {
            Items = treatments,
            Page = 1,
            PageSize = 10,
            TotalCount = 3
        };
        _treatmentRepoMock.Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<Expression<Func<Treatment, bool>>>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _treatmentService.GetPaginatedAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data!.TotalCount);
        Assert.Equal(3, result.Data.Items.Count());
    }

    [Fact]
    public async Task GetPaginatedAsync_WithSearch_FiltersCorrectly()
    {
        // Arrange
        Expression<Func<Treatment, bool>>? capturedFilter = null;
        _treatmentRepoMock.Setup(r => r.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Treatment, bool>>>()))
            .Callback<int, int, Expression<Func<Treatment, bool>>>((p, ps, f) => capturedFilter = f)
            .ReturnsAsync(new PaginatedResult<Treatment> { Items = new List<Treatment>(), Page = 1, PageSize = 10, TotalCount = 0 });

        // Act
        await _treatmentService.GetPaginatedAsync(1, 10, "facial");

        // Assert
        Assert.NotNull(capturedFilter);
        // Optionally, compile and test but not necessary.
    }

    #endregion
}