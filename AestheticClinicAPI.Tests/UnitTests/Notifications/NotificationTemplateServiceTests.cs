using Moq;
using Xunit;
using System.Linq.Expressions;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.Repositories;
using AestheticClinicAPI.Modules.Notifications.Models;
using AestheticClinicAPI.Modules.Notifications.DTOs;

namespace AestheticClinicAPI.Tests.UnitTests.Notifications;

public class NotificationTemplateServiceTests
{
    private readonly Mock<INotificationTemplateRepository> _templateRepoMock;
    private readonly NotificationTemplateService _templateService;

    public NotificationTemplateServiceTests()
    {
        _templateRepoMock = new Mock<INotificationTemplateRepository>();
        _templateService = new NotificationTemplateService(_templateRepoMock.Object);
    }

    private NotificationTemplate CreateSampleTemplate(int id = 1) => new NotificationTemplate
    {
        Id = id,
        Name = "AppointmentReminder",
        Subject = "Reminder: Your appointment is tomorrow",
        Content = "Dear {{ClientName}}, your appointment is on {{AppointmentDate}}."
    };

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsDto()
    {
        // Arrange
        var template = CreateSampleTemplate(1);
        _templateRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

        // Act
        var result = await _templateService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("AppointmentReminder", result.Data?.Name);
    }

    [Fact]
    public async Task GetByNameAsync_Existing_ReturnsDto()
    {
        // Arrange
        var template = CreateSampleTemplate(1);
        _templateRepoMock.Setup(r => r.GetByNameAsync("AppointmentReminder")).ReturnsAsync(template);

        // Act
        var result = await _templateService.GetByNameAsync("AppointmentReminder");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("AppointmentReminder", result.Data?.Name);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_AddsTemplate()
    {
        // Arrange
        var dto = new CreateNotificationTemplateDto
        {
            Name = "WelcomeEmail",
            Subject = "Welcome to our clinic",
            Content = "Hello {{ClientName}}, welcome!"
        };
        NotificationTemplate? captured = null;
        _templateRepoMock.Setup(r => r.AddAsync(It.IsAny<NotificationTemplate>()))
            .Callback<NotificationTemplate>(t => captured = t)
            .ReturnsAsync((NotificationTemplate t) => t);

        // Act
        var result = await _templateService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Name, captured?.Name);
        Assert.Equal(dto.Subject, captured?.Subject);
    }

    [Fact]
    public async Task UpdateAsync_Existing_UpdatesFields()
    {
        // Arrange
        var template = CreateSampleTemplate(1);
        _templateRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);
        _templateRepoMock.Setup(r => r.UpdateAsync(template)).Returns(Task.CompletedTask);
        var dto = new UpdateNotificationTemplateDto
        {
            Name = "UpdatedReminder",
            Subject = "Updated subject",
            Content = "New content"
        };

        // Act
        var result = await _templateService.UpdateAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UpdatedReminder", template.Name);
        Assert.Equal("Updated subject", template.Subject);
        Assert.Equal("New content", template.Content);
    }

    [Fact]
    public async Task DeleteAsync_Existing_Deletes()
    {
        // Arrange
        var template = CreateSampleTemplate(1);
        _templateRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);
        _templateRepoMock.Setup(r => r.DeleteAsync(template)).Returns(Task.CompletedTask);

        // Act
        var result = await _templateService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _templateRepoMock.Verify(r => r.DeleteAsync(template), Times.Once);
    }
}