using AestheticClinicAPI.Modules.Authentications.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Authentications.StateTransitionService;

// Modules/Authentications/StateTransitionService/RefreshTokenStateTransition.cs
public class RefreshTokenStateTransition : IStateTransitionService<RefreshToken>
{
    private readonly ILogger<RefreshTokenStateTransition> _logger;

    public RefreshTokenStateTransition(ILogger<RefreshTokenStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(RefreshToken token, CancellationToken ct = default)
    {
        _logger.LogInformation("[AUTH REFRESH] New refresh token created for User {UserId}, expires at {Expiry}",
            token.UserId, token.ExpiryDate);
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(RefreshToken token, RefreshToken? originalEntity, CancellationToken ct = default)
    {
        // Iwan muna natin – ang revocation ay hahawakan ng OnRevokedChangedAsync
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(RefreshToken token, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(RefreshToken token, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    // Implementasyon para sa revocation
    public Task OnRevokedChangedAsync(RefreshToken token, bool oldRevoked, bool newRevoked, CancellationToken ct = default)
    {
        if (newRevoked && !oldRevoked)
        {
            _logger.LogInformation("[AUTH REFRESH] Refresh token {Id} for User {UserId} has been REVOKED", token.Id, token.UserId);
            // TODO: i-clear ang user session, i-force logout kung kinakailangan
        }
        else if (!newRevoked && oldRevoked)
        {
            _logger.LogWarning("[AUTH REFRESH] Refresh token {Id} for User {UserId} was un-revoked (unusual)", token.Id, token.UserId);
        }
        return Task.CompletedTask;
    }
}