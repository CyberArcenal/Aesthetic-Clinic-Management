using AestheticClinicAPI.Modules.Appointments.Models;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Billing.Services;
using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Modules.Staff.Services;
using AestheticClinicAPI.Modules.Treatments.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Appointments.StateTransitionService;

public class AppointmentStateTransition : IStateTransitionService<Appointment>
{
    private readonly ILogger<AppointmentStateTransition> _logger;
    private readonly INotificationService _notificationService;
    private readonly INotifyLogService _notifyLogService;
    private readonly IInvoiceService _invoiceService;
    private readonly IClientService _clientService;
    private readonly IStaffService _staffService;
    private readonly ITreatmentService _treatmentService;

    public AppointmentStateTransition(
        ILogger<AppointmentStateTransition> logger,
        INotificationService notificationService,
        INotifyLogService notifyLogService,
        IInvoiceService invoiceService,
        IClientService clientService,
        IStaffService staffService,
        ITreatmentService treatmentService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _notifyLogService = notifyLogService;
        _invoiceService = invoiceService;
        _clientService = clientService;
        _staffService = staffService;
        _treatmentService = treatmentService;
    }

    private async Task<(string? name, string? email, string? phone)> GetClientDetails(int clientId)
    {
        var result = await _clientService.GetByIdAsync(clientId);
        return result.IsSuccess && result.Data != null
            ? (result.Data.FullName, result.Data.Email, result.Data.PhoneNumber)
            : (null, null, null);
    }

    private async Task<(string? name, string? email, string? phone)> GetStaffDetails(int? staffId)
    {
        if (!staffId.HasValue) return (null, null, null);
        var result = await _staffService.GetByIdAsync(staffId.Value);
        return result.IsSuccess && result.Data != null
            ? (result.Data.Name, result.Data.Email, result.Data.Phone)
            : (null, null, null);
    }

    private async Task<(string? name, decimal price)> GetTreatmentDetails(int treatmentId)
    {
        var result = await _treatmentService.GetByIdAsync(treatmentId);
        return result.IsSuccess && result.Data != null
            ? (result.Data.Name, result.Data.Price)
            : (null, 0);
    }

    private async Task SendTemplateNotification(string templateName, string recipient, string channel, Dictionary<string, string> metadata)
    {
        var dto = new QueueNotificationDto
        {
            Recipient = recipient,
            Channel = channel,
            Type = templateName,
            Metadata = metadata
        };
        await _notifyLogService.CreateAsync(dto);
    }

    public async Task OnCreatedAsync(Appointment appointment, CancellationToken ct = default)
    {
        _logger.LogInformation("[APPOINTMENT] Created: Id={Id}, ClientId={ClientId}", appointment.Id, appointment.ClientId);

        // Fetch related data
        var (clientName, clientEmail, clientPhone) = await GetClientDetails(appointment.ClientId);
        var (staffName, _, _) = await GetStaffDetails(appointment.StaffId);
        var (treatmentName, _) = await GetTreatmentDetails(appointment.TreatmentId);

        // In-app notification
        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            RecipientId = appointment.ClientId,
            Title = "Appointment Scheduled",
            Message = $"Your {appointment.DurationMinutes}-minute appointment has been scheduled for {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt}.",
            Type = "Info",
            Channel = "InApp",
            ActionUrl = $"/appointments/{appointment.Id}"
        });

        // Email using template
        if (!string.IsNullOrEmpty(clientEmail))
        {
            var metadata = new Dictionary<string, string>
            {
                { "ClientName", clientName ?? "Valued Client" },
                { "TreatmentName", treatmentName ?? "Treatment" },
                { "AppointmentDate", appointment.AppointmentDateTime.ToString("MMMM dd, yyyy") },
                { "AppointmentTime", appointment.AppointmentDateTime.ToString("h:mm tt") },
                { "DurationMinutes", appointment.DurationMinutes.ToString() },
                { "StaffName", staffName ?? "our staff" },
                { "ClinicAddress", "123 Health St., Makati City" },
                { "ClinicName", "Aesthetic Wellness Clinic" },
                { "ClinicPhone", "+63 2 1234 5678" }
            };
            await SendTemplateNotification("AppointmentConfirmation", clientEmail, "Email", metadata);
        }
    }

    public async Task OnUpdatedAsync(Appointment appointment, Appointment? originalEntity, CancellationToken ct = default)
    {
        if (originalEntity == null || originalEntity.AppointmentDateTime == appointment.AppointmentDateTime)
            return;

        _logger.LogInformation("[APPOINTMENT] DateTime changed: {Old} → {New}", originalEntity.AppointmentDateTime, appointment.AppointmentDateTime);

        var (clientName, clientEmail, _) = await GetClientDetails(appointment.ClientId);
        var (staffName, _, _) = await GetStaffDetails(appointment.StaffId);
        var (treatmentName, _) = await GetTreatmentDetails(appointment.TreatmentId);

        // In-app notification
        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            RecipientId = appointment.ClientId,
            Title = "Appointment Rescheduled",
            Message = $"Your appointment has been moved from {originalEntity.AppointmentDateTime:MMMM dd, yyyy h:mm tt} to {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt}.",
            Type = "Warning",
            Channel = "InApp",
            ActionUrl = $"/appointments/{appointment.Id}"
        });

        if (!string.IsNullOrEmpty(clientEmail))
        {
            var metadata = new Dictionary<string, string>
            {
                { "ClientName", clientName ?? "Valued Client" },
                { "TreatmentName", treatmentName ?? "Treatment" },
                { "OldAppointmentDate", originalEntity.AppointmentDateTime.ToString("MMMM dd, yyyy") },
                { "OldAppointmentTime", originalEntity.AppointmentDateTime.ToString("h:mm tt") },
                { "NewAppointmentDate", appointment.AppointmentDateTime.ToString("MMMM dd, yyyy") },
                { "NewAppointmentTime", appointment.AppointmentDateTime.ToString("h:mm tt") },
                { "StaffName", staffName ?? "our staff" },
                { "ClinicName", "Aesthetic Wellness Clinic" },
                { "ClinicPhone", "+63 2 1234 5678" }
            };
            await SendTemplateNotification("AppointmentRescheduled", clientEmail, "Email", metadata);
        }
    }

    public async Task OnStatusChangedAsync(Appointment appointment, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation("[APPOINTMENT] Status changed: {Old} → {New}", oldStatus, newStatus);

        var (clientName, clientEmail, _) = await GetClientDetails(appointment.ClientId);
        var (staffName, _, _) = await GetStaffDetails(appointment.StaffId);
        var (treatmentName, treatmentPrice) = await GetTreatmentDetails(appointment.TreatmentId);

        var metadata = new Dictionary<string, string>
        {
            { "ClientName", clientName ?? "Valued Client" },
            { "TreatmentName", treatmentName ?? "Treatment" },
            { "AppointmentDate", appointment.AppointmentDateTime.ToString("MMMM dd, yyyy") },
            { "AppointmentTime", appointment.AppointmentDateTime.ToString("h:mm tt") },
            { "StaffName", staffName ?? "our staff" },
            { "ClinicName", "Aesthetic Wellness Clinic" },
            { "ClinicPhone", "+63 2 1234 5678" }
        };

        switch (newStatus)
        {
            case "Confirmed":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = appointment.ClientId,
                    Title = "Appointment Confirmed",
                    Message = $"Your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt} has been confirmed.",
                    Type = "Success",
                    Channel = "InApp",
                    ActionUrl = $"/appointments/{appointment.Id}"
                });
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotification("AppointmentConfirmation", clientEmail, "Email", metadata);
                break;

            case "Cancelled":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = appointment.ClientId,
                    Title = "Appointment Cancelled",
                    Message = $"Your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt} has been cancelled.",
                    Type = "Warning",
                    Channel = "InApp",
                    ActionUrl = $"/appointments/{appointment.Id}"
                });
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotification("AppointmentCancelled", clientEmail, "Email", metadata);
                break;

            case "Completed":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = appointment.ClientId,
                    Title = "Appointment Completed",
                    Message = $"Your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt} has been completed.",
                    Type = "Success",
                    Channel = "InApp",
                    ActionUrl = $"/appointments/{appointment.Id}"
                });
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotification("AppointmentCompleted", clientEmail, "Email", metadata);

                // Generate invoice automatically
                await GenerateInvoiceAsync(appointment, treatmentName ?? "Service", treatmentPrice);
                break;

            case "NoShow":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = appointment.ClientId,
                    Title = "Missed Appointment",
                    Message = $"You missed your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt}. A no-show fee may apply.",
                    Type = "Warning",
                    Channel = "InApp",
                    ActionUrl = $"/appointments/{appointment.Id}"
                });
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotification("AppointmentNoShow", clientEmail, "Email", metadata);
                break;
        }
    }

    private async Task GenerateInvoiceAsync(Appointment appointment, string treatmentName, decimal price)
    {
        try
        {
            var dto = new CreateInvoiceDto
            {
                ClientId = appointment.ClientId,
                AppointmentId = appointment.Id,
                IssueDate = DateTime.UtcNow,
                Subtotal = price,
                Tax = 0,
                Notes = $"Invoice for {treatmentName} on {appointment.AppointmentDateTime:MMMM dd, yyyy}"
            };
            var result = await _invoiceService.CreateAsync(dto);
            if (result.IsSuccess)
                _logger.LogInformation("Invoice generated for appointment {AppointmentId}", appointment.Id);
            else
                _logger.LogError("Failed to generate invoice: {Error}", result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for appointment {AppointmentId}", appointment.Id);
        }
    }

    public Task OnActiveChangedAsync(Appointment entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}