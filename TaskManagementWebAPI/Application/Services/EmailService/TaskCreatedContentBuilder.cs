using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskCreatedContentBuilder : ITaskStatusContentBuilder
    {

        private readonly IAppLogger<TaskCreatedContentBuilder> _logger;

        public TaskCreatedContentBuilder(IAppLogger<TaskCreatedContentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public string taskState => "New";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("🆕 New Tasks Assigned:");

                foreach (var task in tasks)
                    sb.AppendLine($" - Task ID: {task.referenceId} + {task.taskType} +  {task.taskName}" +
                        $" (Due: {task.dueDate:MM/dd/yyyy}) " +
                        $"(Priotity: {task.priority})");

                sb.AppendLine();
                return sb.ToString();

            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Failed to build completed task section.");

                throw;
            }

        }
    }
}
