using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AestheticClinicAPI.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AestheticClinicAPI.Middleware;

public class ModelChangeInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<(Type, string), MethodInfo?> _methodCache = new();
    private readonly ILogger<ModelChangeInterceptor> _logger;

    public ModelChangeInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<ModelChangeInterceptor>>();
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔔 [INTERCEPTOR] SavingChangesAsync triggered.");

        if (eventData.Context is not null)
        {
            await DetectAndDispatchChanges(eventData.Context, cancellationToken);
        }

        var saveResult = await base.SavingChangesAsync(eventData, result, cancellationToken);
        _logger.LogInformation("✅ [INTERCEPTOR] Save completed successfully.");
        return saveResult;
    }

    private async Task DetectAndDispatchChanges(DbContext context, CancellationToken ct)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified))
            .ToList();

        if (!entries.Any())
        {
            _logger.LogDebug("📭 [INTERCEPTOR] No added/modified BaseEntity entries found.");
            return;
        }

        _logger.LogInformation("🔍 [INTERCEPTOR] Found {Count} entity(s) to process.", entries.Count);

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var entityType = entity.GetType();
            _logger.LogDebug("   → Processing entity: {EntityType} (Id: {Id}, State: {State})",
                entityType.Name, entity.Id, entry.State);

            var stateServiceType = typeof(IStateTransitionService<>).MakeGenericType(entityType);
            var stateService = _serviceProvider.GetService(stateServiceType);

            if (stateService is null)
            {
                _logger.LogDebug("   ⚠ No state transition service registered for {EntityType}. Skipping.", entityType.Name);
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                _logger.LogInformation("   ➕ {EntityType} CREATED. Dispatching OnCreatedAsync...", entityType.Name);
                await InvokeMethodSafely(stateServiceType, stateService, "OnCreatedAsync", entity, ct);
            }
            else if (entry.State == EntityState.Modified)
            {
                _logger.LogInformation("   ✏️ {EntityType} MODIFIED. Dispatching OnUpdatedAsync...", entityType.Name);
                var originalEntity = CloneEntity(entry.OriginalValues, entityType);
                await InvokeMethodSafely(stateServiceType, stateService, "OnUpdatedAsync", entity, originalEntity, ct);

                // Status property
                var statusProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Status");
                if (statusProp?.IsModified == true)
                {
                    var oldStatus = statusProp.OriginalValue?.ToString() ?? "";
                    var newStatus = statusProp.CurrentValue?.ToString() ?? "";
                    if (oldStatus != newStatus)
                    {
                        _logger.LogInformation("   🔄 Status changed: '{Old}' → '{New}'", oldStatus, newStatus);
                        await InvokeMethodSafely(stateServiceType, stateService, "OnStatusChangedAsync", entity, oldStatus, newStatus, ct);
                    }
                }

                // IsActive property
                var activeProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsActive");
                if (activeProp?.IsModified == true)
                {
                    var oldActive = activeProp.OriginalValue as bool? ?? false;
                    var newActive = activeProp.CurrentValue as bool? ?? false;
                    if (oldActive != newActive)
                    {
                        _logger.LogInformation("   🔄 Active status changed: {Old} → {New}", oldActive, newActive);
                        await InvokeMethodSafely(stateServiceType, stateService, "OnActiveChangedAsync", entity, oldActive, newActive, ct);
                    }
                }

                // IsRevoked property
                var revokedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsRevoked");
                if (revokedProp?.IsModified == true)
                {
                    var oldRevoked = revokedProp.OriginalValue as bool? ?? false;
                    var newRevoked = revokedProp.CurrentValue as bool? ?? false;
                    if (oldRevoked != newRevoked)
                    {
                        _logger.LogInformation("   🔄 Revoked status changed: {Old} → {New}", oldRevoked, newRevoked);
                        await InvokeMethodSafely(stateServiceType, stateService, "OnRevokedChangedAsync", entity, oldRevoked, newRevoked, ct);
                    }
                }
            }
        }
    }

    // Safe version na may try-catch para hindi makaabala sa save
    private async Task InvokeMethodSafely(Type serviceType, object service, string methodName, params object?[] args)
    {
        try
        {
            var key = (serviceType, methodName);
            var method = _methodCache.GetOrAdd(key, static k => k.Item1.GetMethod(k.Item2));
            if (method == null)
            {
                _logger.LogWarning("   ⚠ Method {MethodName} not found on {ServiceType}. Skipping.", methodName, serviceType.Name);
                return;
            }

            _logger.LogDebug("   ▶ Invoking {MethodName} on {ServiceType}...", methodName, serviceType.Name);
            var task = (Task)method.Invoke(service, args)!;
            await task;
            _logger.LogDebug("   ✔ {MethodName} completed successfully.", methodName);
        }
        catch (Exception ex)
        {
            // Log but do NOT rethrow – we don't want to break the transaction
            _logger.LogError(ex, "❌ Error invoking {MethodName} on {ServiceType}. The save operation will continue but the side effect may have failed.",
                methodName, serviceType.Name);
        }
    }

    // Improved cloning using ToObject() (safer)
    private object? CloneEntity(PropertyValues originalValues, Type entityType)
    {
        try
        {
            return originalValues.ToObject();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠ Failed to clone original values for {EntityType}. Original entity will be null.", entityType.Name);
            return null;
        }
    }
}