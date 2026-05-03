using AestheticClinicAPI.Modules.Clients.Models;
using AestheticClinicAPI.Modules.Notifications.Services;
using AestheticClinicAPI.Modules.Notifications.DTOs;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Clients.Services;

public class ClientStateTransition : IStateTransitionService<Client>
{
    private readonly ILogger<ClientStateTransition> _logger;
    private readonly INotifyLogService _notifyLogService;
    private readonly INotificationService _notificationService;

    private readonly IConfiguration _configuration;

    public ClientStateTransition(
        ILogger<ClientStateTransition> logger,
        INotifyLogService notifyLogService,
        INotificationService notificationService,
        IConfiguration configuration)
    {
        _logger = logger;
        _notifyLogService = notifyLogService;
        _notificationService = notificationService;
        _configuration = configuration;
    }

    public async Task OnCreatedAsync(Client client, CancellationToken ct = default)
    {
        _logger.LogInformation("[CLIENT] New client created: {FullName} (ID: {Id}, Email: {Email})",
            $"{client.FirstName} {client.LastName}", client.Id, client.Email);
        var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
        // Send welcome email using template
        var metadata = new Dictionary<string, string>
        {
            { "ClientName", $"{client.FirstName} {client.LastName}" },
            { "Email", client.Email },
            { "ClinicName", "Aesthetic Wellness Clinic" },
            { "PortalUrl", $"{frontendBaseUrl}/login" }
        };
        await _notifyLogService.CreateAsync(new QueueNotificationDto
        {
            Recipient = client.Email,
            Channel = "Email",
            Type = "WelcomeEmail",
            Metadata = metadata
        });

        // In-app notification (if client has a user account, we need UserId; we can't do that here without extra query)
    }

    public async Task OnUpdatedAsync(Client client, Client? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[CLIENT] Client {Id} details updated", client.Id);
        if (originalEntity == null) return;

        if (originalEntity.Email != client.Email)
        {
            _logger.LogInformation("   → Email changed from '{Old}' to '{New}'", originalEntity.Email, client.Email);
            // Send verification email to new address
            await _notifyLogService.CreateAsync(new QueueNotificationDto
            {
                Recipient = client.Email,
                Channel = "Email",
                Type = "EmailChangeRequest",
                Metadata = new Dictionary<string, string>
                {
                    { "OldEmail", originalEntity.Email },
                    { "NewEmail", client.Email },
                    { "ClinicName", "Aesthetic Wellness Clinic" }
                }
            });
        }
        if (originalEntity.PhoneNumber != client.PhoneNumber)
        {
            _logger.LogInformation("   → Phone number changed from '{Old}' to '{New}'", originalEntity.PhoneNumber, client.PhoneNumber);
            // Could send SMS confirmation here
        }
    }

    public Task OnStatusChangedAsync(Client client, string oldStatus, string newStatus, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task OnActiveChangedAsync(Client entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}