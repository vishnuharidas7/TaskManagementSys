using System.Net;
using System.Net.Mail;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class GmailSmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public GmailSmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSection = _config.GetSection("EmailService:GmailSmtp");
            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"]);
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];
            var fromEmail = smtpSection["FromEmail"];

            using var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(fromEmail, to, subject, body);
            await smtpClient.SendMailAsync(mailMessage);

            //throw new NotImplementedException("Service not implemented at the moment...");
        }
    }

}
