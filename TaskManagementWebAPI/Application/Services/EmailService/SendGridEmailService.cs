using LoggingLibrary.Interfaces;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;

        public SendGridEmailService(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey), "apiKey cannot be null.");
            
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var client = new SendGrid.SendGridClient(_apiKey);
                var from = new EmailAddress("noreplytotaskmanagement@gmail.com", "Task Manager");
                var toEmail = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

                var response = await client.SendEmailAsync(msg);

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
            catch (HttpRequestException httpEx)
            {
               
                throw;
            }
            catch (TaskCanceledException timeoutEx)
            {
                
                throw;
            }
            catch (Exception ex)
            {
              
                throw;
            }
        }
    }
}
