using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace AestheticClinicAPI.Modules.Notifications.Channels
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendSimpleEmailAsync(
            string to,
            string subject,
            string message,
            string? from = null
        )
        {
            try
            {
                var smtpSettings = _config.GetSection("Smtp");
                var smtpClient = new SmtpClient(
                    smtpSettings["Host"],
                    int.Parse(smtpSettings["Port"] ?? "587")
                )
                {
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]
                    ),
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(from ?? smtpSettings["From"]),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
