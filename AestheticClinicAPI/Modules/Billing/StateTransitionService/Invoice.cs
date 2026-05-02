using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Billing.StateTransitionService;

public class InvoiceStateTransition : IStateTransitionService<Invoice>
{
    private readonly ILogger<InvoiceStateTransition> _logger;

    public InvoiceStateTransition(ILogger<InvoiceStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Invoice invoice, CancellationToken ct = default)
    {
        _logger.LogInformation("[INVOICE] OnCreatedAsync called for Invoice #{Number}, ClientId: {ClientId}, Total: {Total}", 
            invoice.InvoiceNumber, invoice.ClientId, invoice.Total);
        
        // TODO: 
        // - I-send ang invoice sa client via email/SMS
        // - I-log sa accounting system
        // - Mag-create ng notification para sa client
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Invoice invoice, Invoice? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[INVOICE] OnUpdatedAsync called for Invoice #{Number}", invoice.InvoiceNumber);
        
        if (originalEntity != null)
        {
            // Halimbawa: kung nagbago ang due date
            if (originalEntity.DueDate != invoice.DueDate)
            {
                _logger.LogInformation("   → Due date changed from {Old} to {New}", 
                    originalEntity.DueDate, invoice.DueDate);
                // TODO: magpadala ng updated invoice
            }
            
            // Kung nagbago ang total amount
            if (originalEntity.Total != invoice.Total)
            {
                _logger.LogInformation("   → Total amount changed from {Old} to {New}", 
                    originalEntity.Total, invoice.Total);
                // TODO: mag-issue ng credit note o updated invoice
            }
        }
        
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(Invoice invoice, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation("[INVOICE] Status changed: Invoice #{Number} from '{Old}' → '{New}'", 
            invoice.InvoiceNumber, oldStatus, newStatus);

        switch (newStatus)
        {
            case "Sent":
                // TODO: i-send ang invoice sa client (kung hindi pa na-send sa creation)
                break;
            case "Paid":
                // TODO: i-update ang appointment status (kung may linked appointment)
                // TODO: mag-send ng payment confirmation receipt
                // TODO: i-credit ang package o loyalty points
                break;
            case "Partial":
                // TODO: i-record ang partial payment, mag-send ng reminder para sa remaining balance
                break;
            case "Overdue":
                // TODO: mag-send ng overdue notice at penalty kung applicable
                break;
            case "Cancelled":
                // TODO: i-reverse ang anumang payments, i-notify ang client
                break;
        }
        
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Invoice entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}