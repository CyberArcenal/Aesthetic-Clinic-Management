// Shared/IStateTransitionService.cs
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Shared;

public interface IStateTransitionService<T> where T : BaseEntity
{
    Task OnCreatedAsync(T entity, CancellationToken ct = default);
    Task OnUpdatedAsync(T entity, T? originalEntity, CancellationToken ct = default);
    Task OnStatusChangedAsync(T entity, string oldStatus, string newStatus, CancellationToken ct = default);
    Task OnActiveChangedAsync(T entity, bool oldActive, bool newActive, CancellationToken ct = default);
    
    // Bagong method para sa IsRevoked changes – may default implementation (no-op)
    Task OnRevokedChangedAsync(T entity, bool oldRevoked, bool newRevoked, CancellationToken ct = default)
    {
        // Default: walang ginagawa, para hindi kailangang i-implement ng lahat ng services
        return Task.CompletedTask;
    }
}