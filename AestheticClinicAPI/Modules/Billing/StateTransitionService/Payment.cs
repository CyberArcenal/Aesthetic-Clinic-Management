using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Modules.Billing.Services;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Billing.StateTransitionService;

public class PaymentStateTransition : IStateTransitionService<Payment>
{
    private readonly ILogger<PaymentStateTransition> _logger;
    private readonly IInvoiceService _invoiceService;
    private readonly INotificationService _notificationService;
    private readonly IClientService _clientService;
    private readonly INotifyLogService _notifyLogService;

    public PaymentStateTransition(
        ILogger<PaymentStateTransition> logger,
        IInvoiceService invoiceService,
        INotificationService notificationService,
        IClientService clientService,
        INotifyLogService notifyLogService)
    {
        _logger = logger;
        _invoiceService = invoiceService;
        _notificationService = notificationService;
        _clientService = clientService;
        _notifyLogService = notifyLogService;
    }

    private async Task<(string? name, string? email)> GetClientDetails(int clientId)
    {
        var result = await _clientService.GetByIdAsync(clientId);
        return result.IsSuccess && result.Data != null
            ? (result.Data.FullName, result.Data.Email)
            : (null, null);
    }

    public async Task OnCreatedAsync(Payment payment, CancellationToken ct = default)
    {
        _logger.LogInformation("[PAYMENT] Created: InvoiceId={InvoiceId}, Amount={Amount}, Method={Method}",
            payment.InvoiceId, payment.Amount, payment.Method);

        // Get invoice details
        var invoiceResult = await _invoiceService.GetByIdAsync(payment.InvoiceId);
        if (!invoiceResult.IsSuccess || invoiceResult.Data == null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found for payment {PaymentId}", payment.InvoiceId, payment.Id);
            return;
        }
        var invoice = invoiceResult.Data;

        // Update invoice status based on total paid
        var totalPaidResult = await _invoiceService.GetTotalPaidForInvoiceAsync(payment.InvoiceId);
        decimal totalPaid = totalPaidResult.IsSuccess ? totalPaidResult.Data : 0;
        string newStatus = totalPaid >= invoice.Total ? "Paid" : (totalPaid > 0 ? "Partial" : invoice.Status);
        if (newStatus != invoice.Status)
        {
            await _invoiceService.UpdateStatusAsync(payment.InvoiceId, newStatus);
            _logger.LogInformation("   → Invoice #{Number} status updated to {Status}", invoice.InvoiceNumber, newStatus);
        }

        // In-app notification for the client
        var (clientName, clientEmail) = await GetClientDetails(invoice.ClientId);
        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            RecipientId = invoice.ClientId,
            Title = "Payment Received",
            Message = $"A payment of ₱{payment.Amount:N2} has been applied to invoice #{invoice.InvoiceNumber}. New balance: {(invoice.Total - totalPaid):N2}",
            Type = "Success",
            Channel = "InApp",
            ActionUrl = $"/invoices/{payment.InvoiceId}"
        });

        // Send email receipt
        if (!string.IsNullOrEmpty(clientEmail))
        {
            var metadata = new Dictionary<string, string>
            {
                { "ClientName", clientName ?? "Valued Client" },
                { "InvoiceNumber", invoice.InvoiceNumber },
                { "Amount", payment.Amount.ToString("N2") },
                { "PaymentDate", payment.PaymentDate.ToString("MMMM dd, yyyy") },
                { "PaymentMethod", payment.Method },
                { "BalanceDue", (invoice.Total - totalPaid).ToString("N2") }
            };
            await _notifyLogService.CreateAsync(new QueueNotificationDto
            {
                Recipient = clientEmail,
                Channel = "Email",
                Type = "PaymentReceived",
                Metadata = metadata
            });
        }
    }

    public Task OnUpdatedAsync(Payment payment, Payment? originalEntity, CancellationToken ct = default)
    {
        // Log changes (e.g., reference number updated)
        if (originalEntity != null && originalEntity.ReferenceNumber != payment.ReferenceNumber)
        {
            _logger.LogInformation("   → Reference number changed from '{Old}' to '{New}'",
                originalEntity.ReferenceNumber, payment.ReferenceNumber);
        }
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(Payment payment, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("OnStatusChangedAsync ignored – Payment has no Status property.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Payment entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}