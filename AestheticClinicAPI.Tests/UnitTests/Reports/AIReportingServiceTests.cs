using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Reports.Services;

namespace AestheticClinicAPI.Tests.UnitTests.Reports;

public class AIReportingServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<ILogger<AIReportingService>> _loggerMock;
    private readonly AIReportingService _service;

    public AIReportingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<AIReportingService>>();
        _service = new AIReportingService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GenerateWeeklyReportAsync_CreatesAndSavesReportLog()
    {
        // Act
        var report = await _service.GenerateWeeklyReportAsync();

        // Assert
        Assert.NotNull(report);
        Assert.Equal("WeeklyAIPrediction", report.ReportName);
        Assert.Null(report.GeneratedById);
        Assert.Contains("Weekly AI Prediction Report", report.Insights);
        Assert.True(report.GeneratedAt <= DateTime.UtcNow);

        // Verify saved in database
        var savedReport = await _dbContext.ReportLogs.FirstOrDefaultAsync(r => r.Id == report.Id);
        Assert.NotNull(savedReport);
        Assert.Equal(report.Insights, savedReport.Insights);
    }

    [Fact]
    public async Task GenerateWeeklyReportAsync_GeneratesValidParametersJson()
    {
        // Act
        var report = await _service.GenerateWeeklyReportAsync();

        // Assert
        Assert.NotNull(report.Parameters);
        Assert.Contains("generationDate", report.Parameters);
        Assert.Contains("week", report.Parameters);
        // Should be valid JSON
        Assert.StartsWith("{", report.Parameters);
        Assert.EndsWith("}", report.Parameters);
    }

    [Fact]
    public async Task GenerateWeeklyReportAsync_LogsInformation()
    {
        // Act
        await _service.GenerateWeeklyReportAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting weekly AI prediction report generation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}