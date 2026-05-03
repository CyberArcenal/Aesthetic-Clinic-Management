using AestheticClinicAPI.Modules.Authentications.DTOs;
using AestheticClinicAPI.Modules.Authentications.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.Repositories;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Shared;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Notifications;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notifRepoMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _notifRepoMock = new Mock<INotificationRepository>();
        _userServiceMock = new Mock<IUserService>();
        _notificationService = new NotificationService(
            _notifRepoMock.Object,
            _userServiceMock.Object
        );
    }

    private Notification CreateSampleNotification(int id = 1) =>
        new Notification
        {
            Id = id,
            RecipientId = 1,
            Title = "Test Notification",
            Message = "This is a test",
            Type = "Info",
            Channel = "InApp",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        };

    private UserResponseDto CreateSampleUserDto() =>
        new UserResponseDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
        };

    [Fact]
    public async Task GetByUserAsync_ReturnsNotifications()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            CreateSampleNotification(1),
            CreateSampleNotification(2),
        };
        _notifRepoMock.Setup(r => r.GetByUserAsync(1, 20)).ReturnsAsync(notifications);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var result = await _notificationService.GetByUserAsync(1, 20);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
    }

    [Fact]
    public async Task GetUnreadByUserAsync_ReturnsUnreadNotifications()
    {
        // Arrange
        var unread = new List<Notification> { CreateSampleNotification(1) };
        _notifRepoMock.Setup(r => r.GetUnreadByUserAsync(1)).ReturnsAsync(unread);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var result = await _notificationService.GetUnreadByUserAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
        Assert.False(result.Data!.First().IsRead);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        // Arrange
        _notifRepoMock.Setup(r => r.GetUnreadCountAsync(1)).ReturnsAsync(5);

        // Act
        var result = await _notificationService.GetUnreadCountAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Data);
    }

    [Fact]
    public async Task MarkAsReadAsync_CallsRepository()
    {
        // Arrange
        _notifRepoMock.Setup(r => r.MarkAsReadAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _notificationService.MarkAsReadAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _notifRepoMock.Verify(r => r.MarkAsReadAsync(1), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_CallsRepository()
    {
        // Arrange
        _notifRepoMock.Setup(r => r.MarkAllAsReadAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _notificationService.MarkAllAsReadAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _notifRepoMock.Verify(r => r.MarkAllAsReadAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingNotification_Deletes()
    {
        // Arrange
        var notif = CreateSampleNotification(1);
        _notifRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(notif);
        _notifRepoMock.Setup(r => r.DeleteAsync(notif)).Returns(Task.CompletedTask);

        // Act
        var result = await _notificationService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _notifRepoMock.Verify(r => r.DeleteAsync(notif), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ReturnsFailure()
    {
        // Arrange
        _notifRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Notification?)null);

        // Act
        var result = await _notificationService.DeleteAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Notification not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateNotificationDto
        {
            RecipientId = 1,
            Title = "New Alert",
            Message = "Alert message",
            Type = "Warning",
            Channel = "InApp",
            ActionUrl = "/dashboard",
        };
        Notification? captured = null;
        _notifRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => captured = n)
            .ReturnsAsync((Notification n) => n);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<UserResponseDto>.Success(CreateSampleUserDto()));

        // Act
        var result = await _notificationService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(dto.Title, captured.Title);
        Assert.Equal(dto.Message, captured.Message);
        Assert.False(captured.IsRead);
    }
}
