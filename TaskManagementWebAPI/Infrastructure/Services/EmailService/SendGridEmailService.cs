using SendGrid.Helpers.Mail;
using System.Net;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;

        public SendGridEmailService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var client = new SendGrid.SendGridClient(_apiKey);
            var from = new EmailAddress("noreplytotaskmanagement@gmail.com", "Task Manager");
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);
            // await client.SendEmailAsync(msg);

            var response = await client.SendEmailAsync(msg);

            // Log or check response status
            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("✅ Email sent successfully.");
            }
            else
            {
                Console.WriteLine($"❌ Failed to send email. Status: {response.StatusCode}");
                var responseBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine("Response Body: " + responseBody);
            }
        }
    }
}
