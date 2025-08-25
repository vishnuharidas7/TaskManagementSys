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

            string statusForEmail = tasks.Select(t => t.taskState)
                             .FirstOrDefault(s => s == TaskStatusInfo.New.ToString()) 
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == TaskStatusInfo.Due.ToString()) 
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == TaskStatusInfo.OverDue.ToString()) 
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == TaskStatusInfo.Completed.ToString());

            if (!Enum.TryParse<TaskStatusInfo>(statusForEmail, true, out var parsedStatus))
                return;

            switch (parsedStatus)
            {
                case TaskStatusInfo.New:// "New":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskAssignmentSubject, content);
                    break;
                case TaskStatusInfo.Due: // "Due":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletionReminderSubject, content);
                    break;
                case TaskStatusInfo.OverDue: // "Overdue":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletionReminderSubject, content);
                    break;
                case TaskStatusInfo.Completed: // "Completed":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletedSubject, content);
                    break;
                default:
                    return;
            }
        }
    }
}
