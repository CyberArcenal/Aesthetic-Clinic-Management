using System.Linq.Expressions;
using System.Text.Json;
using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Notifications.Channels;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.Repositories;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.StateTransitionService;
using AestheticClinicAPI.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Notifications;

public class NotifyLogServiceTests
{
    private readonly Mock<INotifyLogRepository> _logRepoMock;
    private readonly Mock<INotificationTemplateService> _templateServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<IPushService> _pushServiceMock;
    private readonly NotifyLogService _notifyLogService;

    // We will use a real in-memory DbContext for the transition (to avoid EF tracking issues)
    private readonly AppDbContext _dbContext;
    private readonly NotifyLogStateTransition _stateTransition;

    public NotifyLogServiceTests()
    {
        _logRepoMock = new Mock<INotifyLogRepository>();
        _templateServiceMock = new Mock<INotificationTemplateService>();
        _emailServiceMock = new Mock<IEmailService>();
        _smsServiceMock = new Mock<ISmsService>();
        _pushServiceMock = new Mock<IPushService>();

        _notifyLogService = new NotifyLogService(
            _logRepoMock.Object,
            _templateServiceMock.Object,
            _emailServiceMock.Object,
            _smsServiceMock.Object,
            _pushServiceMock.Object
        );

        // Setup in-memory DbContext for the state transition
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        _stateTransition = new NotifyLogStateTransition(
            new Mock<ILogger<NotifyLogStateTransition>>().Object,
            _dbContext,
            _templateServiceMock.Object,
            _emailServiceMock.Object,
            _smsServiceMock.Object,
            _pushServiceMock.Object
        );
    }

    private async Task<NotifyLog> InvokeStateTransition(NotifyLog log)
    {
        // Attach the log to the in-memory DbContext so the transition can update it
        _dbContext.NotifyLogs.Add(log);
        await _dbContext.SaveChangesAsync(); // saves the initial "Queued" state
        await _stateTransition.OnCreatedAsync(log);
        await _dbContext.SaveChangesAsync(); // saves the updated status
        return log;
    }

    #region GetByIdAsync Tests
    [Fact]
    public async Task GetByIdAsync_ExistingLog_ReturnsDto()
    {
        var log = new NotifyLog
        {
            Id = 1,
            RecipientEmail = "test@example.com",
            Status = "Queued",
            Channel = "Email",
        };
        _logRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
        var result = await _notifyLogService.GetByIdAsync(1);
        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.Data?.RecipientEmail);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsFailure()
    {
        _logRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((NotifyLog?)null);
        var result = await _notifyLogService.GetByIdAsync(99);
        Assert.False(result.IsSuccess);
        Assert.Equal("Log not found.", result.ErrorMessage);
    }
    #endregion

    #region CreateAsync Tests - Email Channel (Custom)

    [Fact]
    public async Task CreateAsync_EmailChannelCustom_SendsAndUpdatesLog()
    {
        var dto = new QueueNotificationDto
        {
            Recipient = "user@example.com",
            Subject = "Test Subject",
            Message = "Test Message",
            Channel = "Email",
            Type = "custom",
        };

        NotifyLog? createdLog = null;
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<NotifyLog>()))
            .Callback<NotifyLog>(l => createdLog = l)
            .ReturnsAsync((NotifyLog l) => l);

        _emailServiceMock
            .Setup(e =>
                e.SendSimpleEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(true);

        _logRepoMock.Setup(r => r.UpdateAsync(It.IsAny<NotifyLog>())).Returns(Task.CompletedTask);

        var result = await _notifyLogService.CreateAsync(dto);
        Assert.True(result.IsSuccess);

        // Now invoke the state transition (simulate the interceptor)
        var finalLog = await InvokeStateTransition(createdLog!);

        Assert.Equal("Sent", finalLog.Status);
    }

    #endregion

    #region CreateAsync Tests - Email Channel with Template

    [Fact]
    public async Task CreateAsync_EmailWithTemplate_RendersAndSends()
    {
        var dto = new QueueNotificationDto
        {
            Recipient = "user@example.com",
            Channel = "Email",
            Type = "AppointmentReminder",
            Metadata = new Dictionary<string, string>
            {
                { "ClientName", "John" },
                { "AppointmentDate", "2025-05-10" },
            },
        };

        var templateDto = new NotificationTemplateResponseDto
        {
            Id = 1,
            Name = "AppointmentReminder",
            Subject = "Reminder: {{ ClientName }}",
            Content = "Hello {{ ClientName }}, your appointment is on {{ AppointmentDate }}.",
        };

        _templateServiceMock
            .Setup(t => t.GetByNameAsync("AppointmentReminder"))
            .ReturnsAsync(ServiceResult<NotificationTemplateResponseDto>.Success(templateDto));

        _emailServiceMock
            .Setup(e =>
                e.SendSimpleEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(true);

        NotifyLog? createdLog = null;
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<NotifyLog>()))
            .Callback<NotifyLog>(l => createdLog = l)
            .ReturnsAsync((NotifyLog l) => l);

        _logRepoMock.Setup(r => r.UpdateAsync(It.IsAny<NotifyLog>())).Returns(Task.CompletedTask);

        var result = await _notifyLogService.CreateAsync(dto);
        Assert.True(result.IsSuccess);

        var finalLog = await InvokeStateTransition(createdLog!);

        Assert.Equal("Sent", finalLog.Status);
        Assert.Contains("Reminder: John", finalLog.Subject);
        Assert.Contains("Hello John", finalLog.Payload);
    }

    #endregion

    #region CreateAsync Tests - SMS Channel

    [Fact]
    public async Task CreateAsync_SmsChannel_SendsAndUpdates()
    {
        var dto = new QueueNotificationDto
        {
            Recipient = "+639171234567",
            Message = "Your appointment is confirmed",
            Channel = "Sms",
            Type = "custom",
        };

        NotifyLog? createdLog = null;
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<NotifyLog>()))
            .Callback<NotifyLog>(l => createdLog = l)
            .ReturnsAsync((NotifyLog l) => l);

        _smsServiceMock
            .Setup(s => s.SendSmsAsync("+639171234567", "Your appointment is confirmed"))
            .ReturnsAsync(true);

        _logRepoMock.Setup(r => r.UpdateAsync(It.IsAny<NotifyLog>())).Returns(Task.CompletedTask);

        var result = await _notifyLogService.CreateAsync(dto);
        Assert.True(result.IsSuccess);

        var finalLog = await InvokeStateTransition(createdLog!);

        Assert.Equal("Sent", finalLog.Status);
    }

    #endregion
    #region CreateAsync Tests - Email Sending Fails

    [Fact]
    public async Task CreateAsync_EmailSendingFails_UpdatesStatusToFailed()
    {
        var dto = new QueueNotificationDto
        {
            Recipient = "fail@example.com",
            Subject = "Will Fail",
            Message = "This will fail",
            Channel = "Email",
            Type = "custom",
        };

        NotifyLog? createdLog = null;
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<NotifyLog>()))
            .Callback<NotifyLog>(l => createdLog = l)
            .ReturnsAsync((NotifyLog l) => l);

        _emailServiceMock
            .Setup(e =>
                e.SendSimpleEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(false);

        _logRepoMock.Setup(r => r.UpdateAsync(It.IsAny<NotifyLog>())).Returns(Task.CompletedTask);

        var result = await _notifyLogService.CreateAsync(dto);
        Assert.True(result.IsSuccess);

        var finalLog = await InvokeStateTransition(createdLog!);

        Assert.Equal("Failed", finalLog.Status);
    }

    #endregion


    #region RetryAsync Tests

    [Fact]
    public async Task RetryAsync_FailedEmail_ResendsAndUpdatesToSent()
    {
        var log = new NotifyLog
        {
            Id = 1,
            RecipientEmail = "retry@example.com",
            Channel = "Email",
            Status = "Failed",
            Subject = "Retry Subject",
            Payload = "Retry Message",
        };

        _logRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
        // ✅ Include optional 'from' parameter
        _emailServiceMock
            .Setup(e =>
                e.SendSimpleEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(true);
        _logRepoMock.Setup(r => r.UpdateAsync(It.IsAny<NotifyLog>())).Returns(Task.CompletedTask);

        var result = await _notifyLogService.RetryAsync(1);
        Assert.True(result.IsSuccess);
        Assert.Equal("Sent", log.Status);
        Assert.NotNull(log.SentAt);
    }

    [Fact]
    public async Task RetryAsync_NonFailedLog_ReturnsFailure()
    {
        var log = new NotifyLog { Id = 1, Status = "Sent" };
        _logRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
        var result = await _notifyLogService.RetryAsync(1);
        Assert.False(result.IsSuccess);
        Assert.Equal("Only failed logs can be retried.", result.ErrorMessage);
    }

    [Fact]
    public async Task RetryAsync_LogNotFound_ReturnsFailure()
    {
        _logRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((NotifyLog?)null);
        var result = await _notifyLogService.RetryAsync(99);
        Assert.False(result.IsSuccess);
        Assert.Equal("Log not found.", result.ErrorMessage);
    }

    #endregion

    #region GetAllAsync & GetPaginatedAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllLogs()
    {
        var logs = new List<NotifyLog>
        {
            new NotifyLog { Id = 1 },
            new NotifyLog { Id = 2 },
        };
        _logRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(logs);
        var result = await _notifyLogService.GetAllAsync(null);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
    }

    [Fact]
    public async Task GetPaginatedAsync_ReturnsPaginatedResult()
    {
        var logs = new List<NotifyLog> { new NotifyLog { Id = 1 } };
        var paginated = new PaginatedResult<NotifyLog>
        {
            Items = logs,
            Page = 1,
            PageSize = 10,
            TotalCount = 1,
        };
        // Use It.IsAny for the filter (optional), but this method has no optional parameters beyond that.
        // It's fine.
        _logRepoMock
            .Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<Expression<Func<NotifyLog, bool>>>()))
            .ReturnsAsync(paginated);
        var result = await _notifyLogService.GetPaginatedAsync(1, 10);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data!.TotalCount);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingLog_Deletes()
    {
        var log = new NotifyLog { Id = 1 };
        _logRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
        _logRepoMock.Setup(r => r.DeleteAsync(log)).Returns(Task.CompletedTask);
        var result = await _notifyLogService.DeleteAsync(1);
        Assert.True(result.IsSuccess);
        _logRepoMock.Verify(r => r.DeleteAsync(log), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ReturnsFailure()
    {
        _logRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((NotifyLog?)null);
        var result = await _notifyLogService.DeleteAsync(99);
        Assert.False(result.IsSuccess);
        Assert.Equal("Log not found.", result.ErrorMessage);
    }

    #endregion
}
