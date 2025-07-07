using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class NewTaskContentBuilder : ITaskStatusContentBuilder
    {

        private readonly IAppLogger<NewTaskContentBuilder> _logger;

        public NewTaskContentBuilder(IAppLogger<NewTaskContentBuilder> logger)
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
