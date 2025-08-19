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
                             .FirstOrDefault(s => s == TaskStatusEnums.New.ToString()) // "New")
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == TaskStatusEnums.Due.ToString()) //"Due")
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == TaskStatusEnums.OverDue.ToString()) // "Overdue")
                      ?? tasks.Select(t => t.taskState)
                              .FirstOrDefault(s => s == TaskStatusEnums.Completed.ToString()); // "Completed");

            if (!Enum.TryParse<TaskStatusEnums>(statusForEmail, true, out var parsedStatus))
                return;

            switch (parsedStatus)
            {
                case TaskStatusEnums.New:// "New":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskAssignmentSubject, content);
                    break;
                case TaskStatusEnums.Due: // "Due":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletionReminderSubject, content);
                    break;
                case TaskStatusEnums.OverDue: // "Overdue":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletionReminderSubject, content);
                    break;
                case TaskStatusEnums.Completed: // "Completed":
                    await _emailService.SendEmailAsync(user.Email, MailMessages.TaskCompletedSubject, content);
                    break;
                default:
                    return;
            }
        }
    }
}
