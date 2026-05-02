using AestheticClinicAPI.Modules.Billing.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Billing.StateTransitionService;

public class PaymentStateTransition : IStateTransitionService<Payment>
{
    private readonly ILogger<PaymentStateTransition> _logger;

    public PaymentStateTransition(ILogger<PaymentStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Payment payment, CancellationToken ct = default)
    {
        _logger.LogInformation("[PAYMENT] OnCreatedAsync called: InvoiceId {InvoiceId}, Amount {Amount}, Method {Method}", 
            payment.InvoiceId, payment.Amount, payment.Method);
        
        // TODO:
        // - I-validate kung ang payment ay hindi lalampas sa invoice total
        // - I-update ang invoice status (kung fully paid → "Paid", kung partial → "Partial")
        // - Mag-record sa accounting system
        // - Mag-send ng receipt sa client
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Payment payment, Payment? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[PAYMENT] OnUpdatedAsync called for Payment Id {Id}", payment.Id);
        
        if (originalEntity != null)
        {
            // Halimbawa: kung nagbago ang reference number (e.g., after ma-verify ang bank transfer)
            if (originalEntity.ReferenceNumber != payment.ReferenceNumber)
            {
                _logger.LogInformation("   → Reference number changed from '{Old}' to '{New}'", 
                    originalEntity.ReferenceNumber, payment.ReferenceNumber);
                // TODO: i-verify ang payment sa external provider
            }
        }
        
        return Task.CompletedTask;
    }

    // Walang Status property si Payment, kaya panatilihin lang itong placeholder
    public Task OnStatusChangedAsync(Payment payment, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        // Hindi gagamitin dahil walang Status field, pero required ng interface
        _logger.LogDebug("[PAYMENT] OnStatusChangedAsync called but Payment has no Status property. Ignoring.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Payment entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}