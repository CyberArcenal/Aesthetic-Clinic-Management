namespace AestheticClinicAPI.Modules.Notifications.Channels
{
    public interface IEmailService
    {
        Task<bool> SendSimpleEmailAsync(string to, string subject, string message, string? from = null);
    }
}