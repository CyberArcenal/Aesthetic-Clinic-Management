using System.Linq.Expressions;
using AestheticClinicAPI.Modules.Appointments.Constants;
using AestheticClinicAPI.Modules.Appointments.DTOs;
using AestheticClinicAPI.Modules.Appointments.Models;
using AestheticClinicAPI.Modules.Appointments.Repositories;
using AestheticClinicAPI.Modules.Appointments.Services;
using AestheticClinicAPI.Modules.Appointments.StateTransitionService;
using AestheticClinicAPI.Modules.Billing.Services;
using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Staff.Services;
using AestheticClinicAPI.Modules.Treatments.DTOs;
using AestheticClinicAPI.Modules.Treatments.Models;
using AestheticClinicAPI.Modules.Treatments.Services;
using AestheticClinicAPI.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AestheticClinicAPI.Tests.UnitTests.Appointments;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<ITreatmentService> _treatmentServiceMock;
    private readonly Mock<IClientService> _clientServiceMock;
    private readonly Mock<AppointmentStateTransition> _stateTransitionMock;
    private readonly AppointmentService _appointmentService;

    public AppointmentServiceTests()
    {
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _treatmentServiceMock = new Mock<ITreatmentService>();
        _clientServiceMock = new Mock<IClientService>();

        // Create mocks for all dependencies of AppointmentStateTransition
        var loggerMock = new Mock<ILogger<AppointmentStateTransition>>();
        var notificationServiceMock = new Mock<INotificationService>();
        var notifyLogServiceMock = new Mock<INotifyLogService>();
        var invoiceServiceMock = new Mock<IInvoiceService>();
        var staffServiceMock = new Mock<IStaffService>();
        var treatmentServiceMock = new Mock<ITreatmentService>();

        _stateTransitionMock = new Mock<AppointmentStateTransition>(
            MockBehavior.Default,
            loggerMock.Object,
            notificationServiceMock.Object,
            notifyLogServiceMock.Object,
            invoiceServiceMock.Object,
            _clientServiceMock.Object, // IClientService
            staffServiceMock.Object,
            treatmentServiceMock.Object
        );

        _appointmentService = new AppointmentService(
            _appointmentRepoMock.Object,
            _treatmentServiceMock.Object,
            _clientServiceMock.Object,
            _stateTransitionMock.Object
        );
    }

    private Appointment CreateSampleAppointment(int id = 1)
    {
        return new Appointment
        {
            Id = id,
            ClientId = 1,
            TreatmentId = 1,
            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 60,
            Status = AppointmentStatus.Scheduled,
            AssignedStaff = "Dr. Smith",
            Notes = "Test notes",
        };
    }

    private ClientResponseDto CreateSampleClientDto()
    {
        return new ClientResponseDto
        {
            Id = 1,
            FirstName = "Juan",
            LastName = "Dela Cruz",
        };
    }

    private TreatmentResponseDto CreateSampleTreatmentDto()
    {
        return new TreatmentResponseDto
        {
            Id = 1,
            Name = "HydraFacial",
            DurationMinutes = 60,
            Price = 3500,
        };
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsSuccessWithDto()
    {
        // Arrange
        var appointment = CreateSampleAppointment(1);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(appointment);
        _clientServiceMock
            .Setup(c => c.GetClientByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Success(CreateSampleTreatmentDto()));

        // Act
        var result = await _appointmentService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("Juan Dela Cruz", result.Data.ClientName);
        Assert.Equal("HydraFacial", result.Data.TreatmentName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsFailure()
    {
        // Arrange
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Appointment?)null);

        // Act
        var result = await _appointmentService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Appointment not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ClientId = 1,
            TreatmentId = 1,
            AssignedStaff = "Dr. Smith",
            AppointmentDateTime = DateTime.UtcNow.AddDays(2),
            Notes = "First visit",
        };

        var treatmentDto = CreateSampleTreatmentDto();
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Success(treatmentDto));

        Appointment? capturedAppointment = null;
        _appointmentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Appointment>()))
            .Callback<Appointment>(a => capturedAppointment = a)
            .ReturnsAsync((Appointment a) => a);

        _clientServiceMock
            .Setup(c => c.GetClientByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Success(CreateSampleTreatmentDto()));

        // Act
        var result = await _appointmentService.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedAppointment);
        Assert.Equal(AppointmentStatus.Scheduled, capturedAppointment.Status);
        Assert.Equal(60, capturedAppointment.DurationMinutes);
        Assert.Equal(dto.Notes, capturedAppointment.Notes);
    }

    [Fact]
    public async Task CreateAsync_TreatmentNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateAppointmentDto { TreatmentId = 999 };
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(999))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Failure("Treatment not found."));

        // Act
        var result = await _appointmentService.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Treatment not found.", result.ErrorMessage);
        _appointmentRepoMock.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAppointment_UpdatesFields()
    {
        // Arrange
        var appointment = CreateSampleAppointment(1);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(appointment);
        _appointmentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);

        var dto = new UpdateAppointmentDto
        {
            ClientId = 2,
            TreatmentId = 2,
            AssignedStaff = "Dr. Jones",
            AppointmentDateTime = DateTime.UtcNow.AddDays(5),
            Notes = "Updated notes",
        };

        _clientServiceMock
            .Setup(c => c.GetClientByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Success(CreateSampleTreatmentDto()));

        // Act
        var result = await _appointmentService.UpdateAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, appointment.ClientId);
        Assert.Equal(2, appointment.TreatmentId);
        Assert.Equal("Dr. Jones", appointment.AssignedStaff);
        Assert.Equal(dto.Notes, appointment.Notes);
        _appointmentRepoMock.Verify(r => r.UpdateAsync(appointment), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingAppointment_ReturnsFailure()
    {
        // Arrange
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Appointment?)null);
        var dto = new UpdateAppointmentDto();

        // Act
        var result = await _appointmentService.UpdateAsync(99, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Appointment not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_ExistingAppointment_ReturnsSuccess()
    {
        // Arrange
        var appointment = CreateSampleAppointment(1);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(appointment);
        _appointmentRepoMock.Setup(r => r.DeleteAsync(appointment)).Returns(Task.CompletedTask);

        // Act
        var result = await _appointmentService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _appointmentRepoMock.Verify(r => r.DeleteAsync(appointment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingAppointment_ReturnsFailure()
    {
        // Arrange
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Appointment?)null);

        // Act
        var result = await _appointmentService.DeleteAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Appointment not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidStatus_CallsStateTransition()
    {
        // Arrange
        var appointment = CreateSampleAppointment(1);
        appointment.Status = AppointmentStatus.Scheduled;
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(appointment);
        _appointmentRepoMock.Setup(r => r.UpdateAsync(appointment)).Returns(Task.CompletedTask);
        _stateTransitionMock
            .Setup(t =>
                t.OnStatusChangedAsync(
                    appointment,
                    AppointmentStatus.Scheduled,
                    AppointmentStatus.Confirmed,
                    default
                )
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await _appointmentService.UpdateStatusAsync(1, AppointmentStatus.Confirmed);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
        _stateTransitionMock.Verify(
            t =>
                t.OnStatusChangedAsync(
                    appointment,
                    AppointmentStatus.Scheduled,
                    AppointmentStatus.Confirmed,
                    default
                ),
            Times.Once
        );
        _appointmentRepoMock.Verify(r => r.UpdateAsync(appointment), Times.Once);
    }

    [Fact]
    public async Task GetByClientAsync_ReturnsAppointments()
    {
        // Arrange
        var appointments = new List<Appointment>
        {
            CreateSampleAppointment(1),
            CreateSampleAppointment(2),
        };
        _appointmentRepoMock.Setup(r => r.GetByClientAsync(1)).ReturnsAsync(appointments);
        _clientServiceMock
            .Setup(c => c.GetClientByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Success(CreateSampleTreatmentDto()));

        // Act
        var result = await _appointmentService.GetByClientAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsAppointments()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = start.AddDays(7);
        var appointments = new List<Appointment> { CreateSampleAppointment(1) };
        _appointmentRepoMock
            .Setup(r => r.GetByDateRangeAsync(start, end))
            .ReturnsAsync(appointments);
        _clientServiceMock
            .Setup(c => c.GetClientByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Success(CreateSampleTreatmentDto()));

        // Act
        var result = await _appointmentService.GetByDateRangeAsync(start, end);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task CheckAvailabilityAsync_ReturnsAvailability()
    {
        // Arrange
        _appointmentRepoMock
            .Setup(r =>
                r.IsTimeSlotAvailableAsync(
                    It.IsAny<int>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<int?>()
                )
            ) // 4th parameter
            .ReturnsAsync(true);

        // Act
        var result = await _appointmentService.CheckAvailabilityAsync(
            1,
            DateTime.UtcNow.AddHours(1),
            60
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task GetPaginatedAsync_ReturnsPaginatedResult()
    {
        // Arrange
        var appointments = new List<Appointment>
        {
            CreateSampleAppointment(1),
            CreateSampleAppointment(2),
        };
        var paginated = new PaginatedResult<Appointment>
        {
            Items = appointments,
            Page = 1,
            PageSize = 10,
            TotalCount = 2,
        };
        _appointmentRepoMock
            .Setup(r =>
                r.GetPaginatedWithDetailsAsync(
                    1,
                    10,
                    It.IsAny<Expression<Func<Appointment, bool>>>()
                )
            )
            .ReturnsAsync(paginated);
        _clientServiceMock
            .Setup(c => c.GetClientByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));
        _treatmentServiceMock
            .Setup(t => t.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(ServiceResult<TreatmentResponseDto>.Success(CreateSampleTreatmentDto()));

        // Act
        var result = await _appointmentService.GetPaginatedAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Items.Count());
        Assert.Equal(1, result.Data.Page);
        Assert.Equal(2, result.Data.TotalCount);
    }
}
