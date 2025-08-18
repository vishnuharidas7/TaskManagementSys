using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskOverDueEmailContentBuilder : ITaskStatusContentBuilder
    {
        private readonly IAppLogger<TaskOverDueEmailContentBuilder> _logger;

        public TaskOverDueEmailContentBuilder(IAppLogger<TaskOverDueEmailContentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string taskState => "OverDue";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(MailMessages.OverdueReminder);
                sb.AppendLine(MailMessages.TaskDetailsLabel);

                foreach (var task in tasks)
                    sb.AppendLine(string.Format(MailMessages.OverdueTaskLineFormat, task.referenceId, task.taskType, task.taskName, task.dueDate));


                sb.AppendLine(MailMessages.ActionReminder);
                sb.AppendLine();

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Failed to build overdue task section.");
                throw;
            }
        }
    }
}
