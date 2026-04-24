namespace AestheticClinicAPI.Modules.Notifications.Channels
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string toNumber, string message);
    }
}