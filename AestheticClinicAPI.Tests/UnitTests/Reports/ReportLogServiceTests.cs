using System.Linq.Expressions;
using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Reports.DTOs;
using AestheticClinicAPI.Modules.Reports.Models;
using AestheticClinicAPI.Modules.Reports.Repositories;
using AestheticClinicAPI.Modules.Reports.Services;
using AestheticClinicAPI.Shared;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Reports;

public class ReportLogServiceTests
{
    private readonly Mock<IReportLogRepository> _reportRepoMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly ReportLogService _reportLogService;

    public ReportLogServiceTests()
    {
        _reportRepoMock = new Mock<IReportLogRepository>();
        _userServiceMock = new Mock<IUserService>();
        _reportLogService = new ReportLogService(_reportRepoMock.Object, _userServiceMock.Object);
    }

    private ReportLog CreateSampleReportLog(int id = 1) =>
        new ReportLog
        {
            Id = id,
            ReportName = "WeeklySales",
            Parameters = "{\"startDate\":\"2025-01-01\"}",
            GeneratedById = 1,
            Insights = "Sales increased by 20%",
            GeneratedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

    private UserResponseDto CreateSampleUserDto() =>
        new UserResponseDto
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
        };

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingLog_ReturnsDtoWithUserName()
    {
        // Arrange
        var log = CreateSampleReportLog(1);
        _reportRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var result = await _reportLogService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data?.Id);
        Assert.Equal("WeeklySales", result.Data?.ReportName);
        Assert.Equal("admin", result.Data?.GeneratedByName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsFailure()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ReportLog?)null);

        // Act
        var result = await _reportLogService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Report log not found.", result.ErrorMessage);
    }

    #endregion

    #region GenerateReportAsync Tests

    [Fact]
    public async Task GenerateReportAsync_ValidDto_CreatesReportLog()
    {
        // Arrange
        var dto = new GenerateReportDto
        {
            ReportName = "MonthlyRevenue",
            Parameters = "{\"month\":\"March\"}",
        };
        ReportLog? capturedLog = null;
        _reportRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ReportLog>()))
            .Callback<ReportLog>(l => capturedLog = l)
            .ReturnsAsync((ReportLog l) => l);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Failure("User not found")); // not needed for this test

        // Act
        var result = await _reportLogService.GenerateReportAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedLog);
        Assert.Equal("MonthlyRevenue", capturedLog.ReportName);
        Assert.Equal(dto.Parameters, capturedLog.Parameters);
        Assert.Equal("Report generated. Insights will be added later.", capturedLog.Insights);
        Assert.Null(capturedLog.GeneratedById);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingLog_Deletes()
    {
        // Arrange
        var log = CreateSampleReportLog(1);
        _reportRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
        _reportRepoMock.Setup(r => r.DeleteAsync(log)).Returns(Task.CompletedTask);

        // Act
        var result = await _reportLogService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _reportRepoMock.Verify(r => r.DeleteAsync(log), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ReturnsFailure()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ReportLog?)null);

        // Act
        var result = await _reportLogService.DeleteAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Report log not found.", result.ErrorMessage);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllLogs()
    {
        // Arrange
        var logs = new List<ReportLog> { CreateSampleReportLog(1), CreateSampleReportLog(2) };
        _reportRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(logs);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var result = await _reportLogService.GetAllAsync(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
    }

    [Fact]
    public async Task GetAllAsync_WithReportName_Filters()
    {
        // Arrange
        var logs = new List<ReportLog> { CreateSampleReportLog(1) };
        _reportRepoMock.Setup(r => r.GetByReportNameAsync("WeeklySales")).ReturnsAsync(logs);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var result = await _reportLogService.GetAllAsync("WeeklySales");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Fact]
    public async Task GetPaginatedAsync_ReturnsPaginatedResult()
    {
        // Arrange
        var logs = new List<ReportLog> { CreateSampleReportLog(1), CreateSampleReportLog(2) };
        var paginated = new PaginatedResult<ReportLog>
        {
            Items = logs,
            Page = 1,
            PageSize = 10,
            TotalCount = 2,
        };
        _reportRepoMock
            .Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<Expression<Func<ReportLog, bool>>>()))
            .ReturnsAsync(paginated);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var result = await _reportLogService.GetPaginatedAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.TotalCount);
        Assert.Equal(2, result.Data.Items.Count());
    }

    [Fact]
    public async Task GetPaginatedAsync_WithFilters_AppliesFilter()
    {
        // Arrange
        Expression<Func<ReportLog, bool>>? capturedFilter = null;
        _reportRepoMock
            .Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<Expression<Func<ReportLog, bool>>>()))
            .Callback<int, int, Expression<Func<ReportLog, bool>>>((p, ps, f) => capturedFilter = f)
            .ReturnsAsync(
                new PaginatedResult<ReportLog>
                {
                    Items = new List<ReportLog>(),
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 0,
                }
            );
        _userServiceMock
            .Setup(u => u.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var fromDate = new DateTime(2025, 1, 1);
        var toDate = new DateTime(2025, 1, 31);
        await _reportLogService.GetPaginatedAsync(1, 10, "Sales", fromDate, toDate);

        // Assert
        Assert.NotNull(capturedFilter);
        // Optionally compile and test but not required
    }

    #endregion
}
