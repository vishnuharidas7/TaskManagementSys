using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
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

                sb.AppendLine("⏰ Gentle reminder, Task due dates are approaching.");
                sb.AppendLine("\nTask details : \n");

                foreach (var task in tasks)
                    sb.AppendLine($" - Task ID: {task.referenceId} {task.taskType} {task.taskName} (Due: {task.dueDate:MM/dd/yyyy})");

                sb.AppendLine("\nPlease take action on these as soon as possible.");
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
