using AestheticClinicAPI.Modules.Appointments.Models;
using AestheticClinicAPI.Modules.Billing.DTOs;
using AestheticClinicAPI.Modules.Billing.Services;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Staff.Services;
using AestheticClinicAPI.Modules.Treatments.Services;
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

    // Status constants
    private const string StatusConfirmed = "Confirmed";
    private const string StatusCancelled = "Cancelled";
    private const string StatusCompleted = "Completed";
    private const string StatusNoShow = "NoShow";

    public AppointmentStateTransition(
        ILogger<AppointmentStateTransition> logger,
        INotificationService notificationService,
        INotifyLogService notifyLogService,
        IInvoiceService invoiceService,
        IClientService clientService,
        IStaffService staffService,
        ITreatmentService treatmentService
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationService =
            notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _notifyLogService =
            notifyLogService ?? throw new ArgumentNullException(nameof(notifyLogService));
        _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        _staffService = staffService ?? throw new ArgumentNullException(nameof(staffService));
        _treatmentService =
            treatmentService ?? throw new ArgumentNullException(nameof(treatmentService));
    }

    #region Helper Methods

    private async Task<(string? Name, string? Email, string? Phone)> GetClientDetailsAsync(
        int clientId
    )
    {
        var result = await _clientService.GetByIdAsync(clientId);
        return result.IsSuccess && result.Data != null
            ? (result.Data.FullName, result.Data.Email, result.Data.PhoneNumber)
            : (null, null, null);
    }

    private async Task<(string? Name, string? Email, string? Phone)> GetStaffDetailsAsync(
        int? staffId
    )
    {
        if (!staffId.HasValue)
            return (null, null, null);
        var result = await _staffService.GetByIdAsync(staffId.Value);
        return result.IsSuccess && result.Data != null
            ? (result.Data.Name, result.Data.Email, result.Data.Phone)
            : (null, null, null);
    }

    private async Task<(string? Name, decimal Price)> GetTreatmentDetailsAsync(int treatmentId)
    {
        var result = await _treatmentService.GetByIdAsync(treatmentId);
        return result.IsSuccess && result.Data != null
            ? (result.Data.Name, result.Data.Price)
            : (null, 0);
    }

    private Task SendTemplateNotificationAsync(
        string templateName,
        string recipient,
        string channel,
        Dictionary<string, string> metadata,
        CancellationToken ct = default
    )
    {
        var dto = new QueueNotificationDto
        {
            Recipient = recipient,
            Channel = channel,
            Type = templateName,
            Metadata = metadata,
        };
        return _notifyLogService.CreateAsync(dto);
    }

    private async Task SendInAppNotificationAsync(
        int recipientId,
        string title,
        string message,
        string actionUrl
    )
    {
        await _notificationService.CreateAsync(
            new CreateNotificationDto
            {
                RecipientId = recipientId,
                Title = title,
                Message = message,
                Type = "Info", // Can be overridden per call if needed
                Channel = "InApp",
                ActionUrl = actionUrl,
            }
        );
    }

    private Dictionary<string, string> BuildBaseMetadata(
        string clientName,
        string treatmentName,
        DateTime appointmentDate,
        string staffName,
        string? additionalKeys = null
    )
    {
        var metadata = new Dictionary<string, string>
        {
            ["ClientName"] = clientName ?? "Valued Client",
            ["TreatmentName"] = treatmentName ?? "Treatment",
            ["AppointmentDate"] = appointmentDate.ToString("MMMM dd, yyyy"),
            ["AppointmentTime"] = appointmentDate.ToString("h:mm tt"),
            ["StaffName"] = staffName ?? "our staff",
            ["ClinicName"] = "Aesthetic Wellness Clinic",
            ["ClinicPhone"] = "+63 2 1234 5678",
        };
        return metadata;
    }

    #endregion

    #region IStateTransitionService Implementation

    public async Task OnCreatedAsync(Appointment appointment, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[APPOINTMENT] Created: Id={Id}, ClientId={ClientId}",
            appointment.Id,
            appointment.ClientId
        );

        var (clientName, clientEmail, _) = await GetClientDetailsAsync(appointment.ClientId);
        var (staffName, _, _) = await GetStaffDetailsAsync(appointment.StaffId);
        var (treatmentName, _) = await GetTreatmentDetailsAsync(appointment.TreatmentId);

        // In-app notification
        await SendInAppNotificationAsync(
            appointment.ClientId,
            "Appointment Scheduled",
            $"Your {appointment.DurationMinutes}-minute appointment has been scheduled for {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt}.",
            $"/appointments/{appointment.Id}"
        );

        // Email via template
        if (!string.IsNullOrEmpty(clientEmail))
        {
            var metadata = BuildBaseMetadata(
                clientName,
                treatmentName,
                appointment.AppointmentDateTime,
                staffName
            );
            metadata["ClinicAddress"] = "123 Health St., Makati City";
            metadata["DurationMinutes"] = appointment.DurationMinutes.ToString();

            await SendTemplateNotificationAsync(
                "AppointmentConfirmation",
                clientEmail,
                "Email",
                metadata,
                ct
            );
        }
    }

    public async Task OnUpdatedAsync(
        Appointment appointment,
        Appointment? originalEntity,
        CancellationToken ct = default
    )
    {
        if (originalEntity?.AppointmentDateTime == appointment.AppointmentDateTime)
            return;

        _logger.LogInformation(
            "[APPOINTMENT] DateTime changed: {Old} → {New}",
            originalEntity?.AppointmentDateTime,
            appointment.AppointmentDateTime
        );

        var (clientName, clientEmail, _) = await GetClientDetailsAsync(appointment.ClientId);
        var (staffName, _, _) = await GetStaffDetailsAsync(appointment.StaffId);
        var (treatmentName, _) = await GetTreatmentDetailsAsync(appointment.TreatmentId);

        // In-app notification
        await SendInAppNotificationAsync(
            appointment.ClientId,
            "Appointment Rescheduled",
            $"Your appointment has been moved from {originalEntity!.AppointmentDateTime:MMMM dd, yyyy h:mm tt} to {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt}.",
            $"/appointments/{appointment.Id}"
        );

        // Email via template
        if (!string.IsNullOrEmpty(clientEmail))
        {
            var metadata = BuildBaseMetadata(
                clientName,
                treatmentName,
                appointment.AppointmentDateTime,
                staffName
            );
            metadata["OldAppointmentDate"] = originalEntity.AppointmentDateTime.ToString(
                "MMMM dd, yyyy"
            );
            metadata["OldAppointmentTime"] = originalEntity.AppointmentDateTime.ToString("h:mm tt");
            metadata["NewAppointmentDate"] = appointment.AppointmentDateTime.ToString(
                "MMMM dd, yyyy"
            );
            metadata["NewAppointmentTime"] = appointment.AppointmentDateTime.ToString("h:mm tt");

            await SendTemplateNotificationAsync(
                "AppointmentRescheduled",
                clientEmail,
                "Email",
                metadata,
                ct
            );
        }
    }

    public virtual async Task OnStatusChangedAsync(
        Appointment appointment,
        string oldStatus,
        string newStatus,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("[APPOINTMENT] Status changed: {Old} → {New}", oldStatus, newStatus);

        var (clientName, clientEmail, _) = await GetClientDetailsAsync(appointment.ClientId);
        var (staffName, _, _) = await GetStaffDetailsAsync(appointment.StaffId);
        var (treatmentName, treatmentPrice) = await GetTreatmentDetailsAsync(
            appointment.TreatmentId
        );

        var metadata = BuildBaseMetadata(
            clientName,
            treatmentName,
            appointment.AppointmentDateTime,
            staffName
        );

        switch (newStatus)
        {
            case StatusConfirmed:
                await SendInAppNotificationAsync(
                    appointment.ClientId,
                    "Appointment Confirmed",
                    $"Your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt} has been confirmed.",
                    $"/appointments/{appointment.Id}"
                );
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotificationAsync(
                        "AppointmentConfirmation",
                        clientEmail,
                        "Email",
                        metadata,
                        ct
                    );
                break;

            case StatusCancelled:
                await SendInAppNotificationAsync(
                    appointment.ClientId,
                    "Appointment Cancelled",
                    $"Your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt} has been cancelled.",
                    $"/appointments/{appointment.Id}"
                );
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotificationAsync(
                        "AppointmentCancelled",
                        clientEmail,
                        "Email",
                        metadata,
                        ct
                    );
                break;

            case StatusCompleted:
                await SendInAppNotificationAsync(
                    appointment.ClientId,
                    "Appointment Completed",
                    $"Your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt} has been completed.",
                    $"/appointments/{appointment.Id}"
                );
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotificationAsync(
                        "AppointmentCompleted",
                        clientEmail,
                        "Email",
                        metadata,
                        ct
                    );

                await GenerateInvoiceAsync(appointment, treatmentName ?? "Service", treatmentPrice);
                break;

            case StatusNoShow:
                await SendInAppNotificationAsync(
                    appointment.ClientId,
                    "Missed Appointment",
                    $"You missed your appointment on {appointment.AppointmentDateTime:MMMM dd, yyyy h:mm tt}. A no-show fee may apply.",
                    $"/appointments/{appointment.Id}"
                );
                if (!string.IsNullOrEmpty(clientEmail))
                    await SendTemplateNotificationAsync(
                        "AppointmentNoShow",
                        clientEmail,
                        "Email",
                        metadata,
                        ct
                    );
                break;
        }
    }

    private async Task GenerateInvoiceAsync(
        Appointment appointment,
        string treatmentName,
        decimal price
    )
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
                Notes =
                    $"Invoice for {treatmentName} on {appointment.AppointmentDateTime:MMMM dd, yyyy}",
            };
            var result = await _invoiceService.CreateAsync(dto);
            if (result.IsSuccess)
                _logger.LogInformation(
                    "Invoice generated for appointment {AppointmentId}",
                    appointment.Id
                );
            else
                _logger.LogError("Failed to generate invoice: {Error}", result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating invoice for appointment {AppointmentId}",
                appointment.Id
            );
        }
    }

    public Task OnActiveChangedAsync(
        Appointment entity,
        bool oldActive,
        bool newActive,
        CancellationToken ct = default
    ) => Task.CompletedTask;

    #endregion
}
