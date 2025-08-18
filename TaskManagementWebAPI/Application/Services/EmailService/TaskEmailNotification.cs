using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskEmailNotification : ITaskNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IEmailContentBuilder _emailContentBuilder;
        
        public TaskEmailNotification(IEmailService emailService, IEmailContentBuilder emailContentBuilder)
        {
            _emailService = emailService;
            _emailContentBuilder = emailContentBuilder;
        }

        public async Task SendNotificationAsync(Users user, IEnumerable<Tasks> tasks)
        {
            if (!tasks.Any())
                return;

            var content = _emailContentBuilder.BuildContent(user, tasks);
            //await _emailService.SendEmailAsync(user.Email, "New Task Added", content);
            string statusForEmail = tasks.Select(t => t.taskState)
                             .FirstOrDefault(s => s == "New")
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == "Due")
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == "Overdue")
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == "Completed");

            switch (statusForEmail)
            {
                case "New":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskAssignmentSubject, content);
                    break;
                case "Due":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletionReminderSubject, content);
                    break;
                case "Overdue":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletionReminderSubject, content);
                    break;
                case "Completed":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletedSubject, content);
                    break;
                default:
                    return;
            }
        }
    }
}
