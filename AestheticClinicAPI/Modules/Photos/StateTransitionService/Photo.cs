using AestheticClinicAPI.Modules.Photos.Models;
using AestheticClinicAPI.Shared;

namespace AestheticClinicAPI.Modules.Photos.StateTransitionService;

public class PhotoStateTransition : IStateTransitionService<Photo>
{
    private readonly ILogger<PhotoStateTransition> _logger;

    public PhotoStateTransition(ILogger<PhotoStateTransition> logger)
    {
        _logger = logger;
    }

    public Task OnCreatedAsync(Photo photo, CancellationToken ct = default)
    {
        _logger.LogInformation("[PHOTO] New photo created for ClientId: {ClientId}, AppointmentId: {AppointmentId}, IsBefore: {IsBefore}, FileName: {FileName}",
            photo.ClientId, photo.AppointmentId, photo.IsBefore, photo.FileName);
        
        // TODO:
        // - I-compress o i-optimize ang larawan (kung hindi pa)
        // - Mag-generate ng thumbnail
        // - I-scan para sa malware / NSFW content (kung kinakailangan)
        // - I-update ang client record na may bagong photo
        // - I-notify ang staff (kung after photo, for assessment)
        
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Photo photo, Photo? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[PHOTO] Photo {Id} updated for ClientId: {ClientId}", photo.Id, photo.ClientId);
        
        if (originalEntity != null)
        {
            // Kung nagbago ang file path (ibig sabihin, pinalitan ang larawan)
            if (originalEntity.FilePath != photo.FilePath)
            {
                _logger.LogInformation("   → File path changed from '{OldPath}' to '{NewPath}'", 
                    originalEntity.FilePath, photo.FilePath);
                // TODO: i-delete ang lumang file (kung hindi na kailangan)
                // TODO: mag-recompress ng bagong file
            }
            
            // Kung nagbago ang description
            if (originalEntity.Description != photo.Description)
            {
                _logger.LogInformation("   → Description updated: '{NewDesc}'", photo.Description);
                // TODO: i-update ang metadata sa storage
            }
            
            // Kung nagbago ang before/after flag
            if (originalEntity.IsBefore != photo.IsBefore)
            {
                _logger.LogInformation("   → Photo type changed from {OldType} to {NewType}", 
                    originalEntity.IsBefore ? "Before" : "After", 
                    photo.IsBefore ? "Before" : "After");
                // TODO: i-reorganize sa storage folder (before vs after)
            }
        }
        
        return Task.CompletedTask;
    }

    // Walang Status property si Photo, placeholder lang
    public Task OnStatusChangedAsync(Photo photo, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogDebug("[PHOTO] OnStatusChangedAsync called but Photo has no Status field. Ignoring.");
        return Task.CompletedTask;
    }

    public Task OnActiveChangedAsync(Photo entity, bool oldActive, bool newActive, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}