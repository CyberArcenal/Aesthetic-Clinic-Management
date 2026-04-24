namespace AestheticClinicAPI.Modules.Notifications.Channels
{
    public interface IPushService
    {
        Task<bool> SendPushAsync(string deviceToken, string title, string message);
    }
}