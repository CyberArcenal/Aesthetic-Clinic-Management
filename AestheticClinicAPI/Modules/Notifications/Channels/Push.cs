using Microsoft.Extensions.Logging;

namespace AestheticClinicAPI.Modules.Notifications.Channels
{
    public class PushService : IPushService
    {
        private readonly ILogger<PushService> _logger;

        public PushService(ILogger<PushService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendPushAsync(string deviceToken, string title, string message)
        {
            _logger.LogInformation("[PUSH] To: {Token}, Title: {Title}, Message: {Message}", deviceToken, title, message);
            return Task.FromResult(true);
        }
    }
}