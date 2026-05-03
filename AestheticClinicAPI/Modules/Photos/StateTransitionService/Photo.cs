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
        _logger.LogInformation("[PHOTO] New photo saved: Client {ClientId}, File {FileName}, Before/After: {IsBefore}",
            photo.ClientId, photo.FileName, photo.IsBefore);
        // Actual file manipulation (compression, thumbnail) should be done in the service before saving.
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(Photo photo, Photo? originalEntity, CancellationToken ct = default)
    {
        _logger.LogInformation("[PHOTO] Photo {Id} updated", photo.Id);
        if (originalEntity != null && originalEntity.FilePath != photo.FilePath)
        {
            _logger.LogInformation("   → File replaced. Old file should be deleted by the service.");
            // Deletion of old physical file is handled in the service layer (PhotoService.DeleteAsync)
        }
        return Task.CompletedTask;
    }

    public Task OnStatusChangedAsync(Photo photo, string oldStatus, string newStatus, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task OnActiveChangedAsync(Photo entity, bool oldActive, bool newActive, CancellationToken ct = default)
        => Task.CompletedTask;
}