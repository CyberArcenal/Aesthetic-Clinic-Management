using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Clients.Services;

public class ClientStateTransition : IStateTransitionService<Client>
{
    private readonly ILogger<ClientStateTransition> _logger;

    public ClientStateTransition(ILogger<ClientStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Client client, CancellationToken ct = default)
    {
        _logger.LogInformation("[CLIENT] New client created: {FullName} (ID: {Id}, Email: {Email})", 
            $"{client.FirstName} {client.LastName}", client.Id, client.Email);
        
        // TODO:
        // - Magpadala ng welcome email
        // - Mag-create ng default notification preferences
        // - I-add sa mailing list (kung pumayag)
        // - Mag-generate ng client QR code / loyalty card
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Client client, Client? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[CLIENT] Client {Id} details updated", client.Id);
        
        if (originalEntity != null)
        {
            // Check kung may binagong email
            if (originalEntity.Email != client.Email)
            {
                _logger.LogInformation("   → Email changed from '{Old}' to '{New}'", 
                    originalEntity.Email, client.Email);
                // TODO: magpadala ng verification sa bagong email
            }
            
            // Check kung may binagong phone number
            if (originalEntity.PhoneNumber != client.PhoneNumber)
            {
                _logger.LogInformation("   → Phone number changed from '{Old}' to '{New}'", 
                    originalEntity.PhoneNumber, client.PhoneNumber);
                // TODO: i-update sa SMS notification system
            }
            
            // Check kung may binagong skin history o allergies
            if (originalEntity.SkinHistory != client.SkinHistory)
            {
                _logger.LogInformation("   → Skin history updated");
                // TODO: i-flag para sa treatment recommendation
            }
            
            if (originalEntity.Allergies != client.Allergies)
            {
                _logger.LogInformation("   → Allergies updated");
                // TODO: i-check sa upcoming appointments para maiwasan ang allergic reactions
            }
        }
        
        return Task.CompletedTask;
    }

    // Pansin: Ang Client model ay walang "Status" property (hal. IsActive lang).
    // Pwede nating i-monitor ang IsActive bilang "status" kung gusto.
    // Pero para sa interface compliance, iiwan muna natin ito.
    public Task OnStatusChangedAsync(Client client, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        // Puwede nating i-extend: kung magkakaroon ng Status property (Active/Inactive) sa hinaharap.
        _logger.LogDebug("[CLIENT] OnStatusChangedAsync called but Client has no explicit Status field. Ignoring.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Client entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}