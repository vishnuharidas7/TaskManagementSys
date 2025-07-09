using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
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
                sb.AppendLine("⏰ This is a reminder that the following task exceeded due date and require your attention:");
                sb.AppendLine("\nTask details : \n");

                foreach (var task in tasks)
                    sb.AppendLine($" - Task ID: {task.referenceId} {task.taskType} {task.taskName} (Due: {task.dueDate:MM/dd/yyyy})");

                sb.AppendLine("\nPlease take action on these as soon as possible.");
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
