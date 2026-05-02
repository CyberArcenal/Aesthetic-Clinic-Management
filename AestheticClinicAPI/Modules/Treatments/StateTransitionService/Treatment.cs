using AestheticClinicAPI.Modules.Treatments.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Treatments.StateTransitionService;

public class TreatmentStateTransition : IStateTransitionService<Treatment>
{
    private readonly ILogger<TreatmentStateTransition> _logger;

    public TreatmentStateTransition(ILogger<TreatmentStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Treatment treatment, CancellationToken ct = default)
    {
        _logger.LogInformation("[TREATMENT] New treatment created: '{Name}' (ID: {Id}, Category: {Category}, Price: {Price})",
            treatment.Name, treatment.Id, treatment.Category, treatment.Price);

        // TODO:
        // - I-validate kung may duplicate na pangalan
        // - I-add sa treatment catalog cache
        // - Mag-create ng default package offers (kung applicable)
        // - I-notify ang staff ng bagong serbisyo

        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Treatment treatment, Treatment? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[TREATMENT] Treatment {Id} ('{Name}') updated", treatment.Id, treatment.Name);

        if (originalEntity != null)
        {
            // Kung nagbago ang presyo
            if (originalEntity.Price != treatment.Price)
            {
                _logger.LogInformation("   → Price changed from {Old:C} to {New:C}", originalEntity.Price, treatment.Price);
                // TODO: i-update ang mga active package/promo na gumagamit ng treatment na ito
                // TODO: i-notify ang sales team
            }

            // Kung nagbago ang duration
            if (originalEntity.DurationMinutes != treatment.DurationMinutes)
            {
                _logger.LogInformation("   → Duration changed from {Old} min to {New} min",
                    originalEntity.DurationMinutes, treatment.DurationMinutes);
                // TODO: i-adjust ang scheduling slots
            }

            // Kung nagbago ang category
            if (originalEntity.Category != treatment.Category)
            {
                _logger.LogInformation("   → Category changed from '{Old}' to '{New}'",
                    originalEntity.Category, treatment.Category);
                // TODO: i-reorganize sa catalog
            }

            // Kung nagbago ang description
            if (originalEntity.Description != treatment.Description)
            {
                _logger.LogInformation("   → Description updated");
                // TODO: i-update sa marketing materials
            }
        }

        return Task.CompletedTask;
    }

    // Suportahan ang IsActive bilang status (Active/Inactive)
    public Task OnStatusChangedAsync(Treatment treatment, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation("[TREATMENT] Treatment '{Name}' (ID: {Id}) status changed from '{Old}' → '{New}'",
            treatment.Name, treatment.Id, oldStatus, newStatus);

        switch (newStatus)
        {
            case "Active":
                // TODO: i-activate sa booking system, i-display sa catalog
                break;
            case "Inactive":
                // TODO: i-hide sa catalog, i-prevent ang bagong booking, i-handle ang existing appointments
                break;
        }

        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Treatment entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}