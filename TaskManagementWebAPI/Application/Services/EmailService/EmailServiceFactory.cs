using System.Net;
using System.Net.Mail;
using TaskManagementWebAPI.Application.Interfaces;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public static class EmailServiceFactory
    {
        public static IEmailService CreateEmailService(IConfiguration config)
        {

            var provider = config["EmailService:Type"];

            switch (provider)
            {
                case "SendGrid":
                    // Note: won't work unless domain is verified
                    return new SendGridEmailService(config["EmailService:SendGrid:ApiKey"]);

                case "GmailSmtp":
                    return new GmailSmtpEmailService(config);

                default:
                    throw new NotSupportedException($"Email provider '{provider}' is not supported.");
            }
        }
    }
}
