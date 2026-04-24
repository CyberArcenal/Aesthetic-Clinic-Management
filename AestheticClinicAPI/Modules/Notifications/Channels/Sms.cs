using Microsoft.Extensions.Logging;

namespace AestheticClinicAPI.Modules.Notifications.Channels
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendSmsAsync(string toNumber, string message)
        {
            // Placeholder – replace with actual SMS provider (Twilio, etc.)
            _logger.LogInformation("[SMS] To: {To}, Message: {Message}", toNumber, message);
            return Task.FromResult(true);
        }
    }
}