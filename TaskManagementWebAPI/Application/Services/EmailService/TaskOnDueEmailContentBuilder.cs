using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskOnDueEmailContentBuilder : ITaskStatusContentBuilder
    {
        private readonly IAppLogger<TaskOnDueEmailContentBuilder> _logger;

        public TaskOnDueEmailContentBuilder(IAppLogger<TaskOnDueEmailContentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string taskState => "Due";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine(MailMessages.ReminderHeader);
                sb.AppendLine(MailMessages.TaskDetailsLabel);

                foreach (var task in tasks)
                    sb.AppendLine(string.Format(MailMessages.TaskLineFormat,
                                  task.referenceId, task.taskType, task.taskName, task.dueDate));

                sb.AppendLine(MailMessages.ReminderClosing);
                sb.AppendLine();

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Error while building OnDue task section.");
                throw;
            }
        }
    }

}
