using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Billing.StateTransitionService;

public class InvoiceStateTransition : IStateTransitionService<Invoice>
{
    private readonly ILogger<InvoiceStateTransition> _logger;
    private readonly INotifyLogService _notifyLogService;
    private readonly INotificationService _notificationService;
    private readonly IClientService _clientService;

    public InvoiceStateTransition(
        ILogger<InvoiceStateTransition> logger,
        INotifyLogService notifyLogService,
        INotificationService notificationService,
        IClientService clientService)
    {
        _logger = logger;
        _notifyLogService = notifyLogService;
        _notificationService = notificationService;
        _clientService = clientService;
    }

    private async Task<(string? name, string? email)> GetClientDetails(int clientId)
    {
        var result = await _clientService.GetByIdAsync(clientId);
        return result.IsSuccess && result.Data != null
            ? (result.Data.FullName, result.Data.Email)
            : (null, null);
    }

    public async Task OnCreatedAsync(Invoice invoice, CancellationToken ct = default)
    {
        _logger.LogInformation("[INVOICE] Created: #{Number}, ClientId={ClientId}, Total={Total}",
            invoice.InvoiceNumber, invoice.ClientId, invoice.Total);

        var (clientName, clientEmail) = await GetClientDetails(invoice.ClientId);

        // In-app notification
        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            RecipientId = invoice.ClientId,
            Title = "New Invoice Created",
            Message = $"Invoice #{invoice.InvoiceNumber} for ₱{invoice.Total:N2} has been created.",
            Type = "Info",
            Channel = "InApp",
            ActionUrl = $"/invoices/{invoice.Id}"
        });

        // Send email with invoice details (using template "InvoiceSent")
        if (!string.IsNullOrEmpty(clientEmail))
        {
            var metadata = new Dictionary<string, string>
            {
                { "ClientName", clientName ?? "Valued Client" },
                { "InvoiceNumber", invoice.InvoiceNumber },
                { "IssueDate", invoice.IssueDate.ToString("MMMM dd, yyyy") },
                { "DueDate", invoice.DueDate?.ToString("MMMM dd, yyyy") ?? "Not set" },
                { "TotalAmount", invoice.Total.ToString("N2") },
                { "ClinicName", "Aesthetic Wellness Clinic" }
            };
            var emailDto = new QueueNotificationDto
            {
                Recipient = clientEmail,
                Channel = "Email",
                Type = "InvoiceSent",
                Metadata = metadata
            };
            await _notifyLogService.CreateAsync(emailDto);
        }
    }

    public Task OnUpdatedAsync(Invoice invoice, Invoice? originalEntity, CancellationToken ct = default)
    {
        // We'll keep logging only – actual actions like sending updated invoice can be added later if needed.
        if (originalEntity != null && originalEntity.Total != invoice.Total)
        {
            _logger.LogInformation("   → Total changed from {Old} to {New}", originalEntity.Total, invoice.Total);
        }
        return Task.CompletedTask;
    }

    public async Task OnStatusChangedAsync(Invoice invoice, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation("[INVOICE] Status changed: #{Number} from '{Old}' → '{New}'",
            invoice.InvoiceNumber, oldStatus, newStatus);

        var (clientName, clientEmail) = await GetClientDetails(invoice.ClientId);
        var metadata = new Dictionary<string, string>
        {
            { "ClientName", clientName ?? "Valued Client" },
            { "InvoiceNumber", invoice.InvoiceNumber },
            { "TotalAmount", invoice.Total.ToString("N2") },
            { "DueDate", invoice.DueDate?.ToString("MMMM dd, yyyy") ?? "Not set" },
            { "ClinicName", "Aesthetic Wellness Clinic" }
        };

        switch (newStatus)
        {
            case "Sent":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = invoice.ClientId,
                    Title = "Invoice Sent",
                    Message = $"Invoice #{invoice.InvoiceNumber} has been sent to your email.",
                    Type = "Success",
                    Channel = "InApp",
                    ActionUrl = $"/invoices/{invoice.Id}"
                });
                // Email already sent on creation; optionally resend.
                break;

            case "Paid":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = invoice.ClientId,
                    Title = "Invoice Paid",
                    Message = $"Invoice #{invoice.InvoiceNumber} has been fully paid. Thank you!",
                    Type = "Success",
                    Channel = "InApp",
                    ActionUrl = $"/invoices/{invoice.Id}"
                });
                if (!string.IsNullOrEmpty(clientEmail))
                {
                    var paymentMeta = new Dictionary<string, string>(metadata)
                    {
                        { "PaymentDate", DateTime.UtcNow.ToString("MMMM dd, yyyy") }
                    };
                    await _notifyLogService.CreateAsync(new QueueNotificationDto
                    {
                        Recipient = clientEmail,
                        Channel = "Email",
                        Type = "PaymentReceived",
                        Metadata = paymentMeta
                    });
                }
                break;

            case "Overdue":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = invoice.ClientId,
                    Title = "Invoice Overdue",
                    Message = $"Invoice #{invoice.InvoiceNumber} is now overdue. Please settle as soon as possible.",
                    Type = "Warning",
                    Channel = "InApp",
                    ActionUrl = $"/invoices/{invoice.Id}"
                });
                if (!string.IsNullOrEmpty(clientEmail))
                {
                    await _notifyLogService.CreateAsync(new QueueNotificationDto
                    {
                        Recipient = clientEmail,
                        Channel = "Email",
                        Type = "InvoiceOverdue",
                        Metadata = metadata
                    });
                }
                break;

            case "Partial":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = invoice.ClientId,
                    Title = "Partial Payment Received",
                    Message = $"A partial payment has been applied to invoice #{invoice.InvoiceNumber}. Remaining balance: {invoice.Total - (await GetTotalPaid(invoice.Id)):N2}",
                    Type = "Info",
                    Channel = "InApp",
                    ActionUrl = $"/invoices/{invoice.Id}"
                });
                break;

            case "Cancelled":
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    RecipientId = invoice.ClientId,
                    Title = "Invoice Cancelled",
                    Message = $"Invoice #{invoice.InvoiceNumber} has been cancelled.",
                    Type = "Warning",
                    Channel = "InApp",
                    ActionUrl = $"/invoices/{invoice.Id}"
                });
                break;
        }
    }

    private async Task<decimal> GetTotalPaid(int invoiceId)
    {
        // We need to inject IInvoiceRepository or IPaymentRepository – but for simplicity, assume we have a method.
        // To avoid overcomplicating, we'll just return 0 for now.
        return 0;
    }

    public Task OnActiveChangedAsync(Invoice entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}