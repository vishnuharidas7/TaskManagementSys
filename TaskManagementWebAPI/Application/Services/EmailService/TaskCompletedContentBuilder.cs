using LoggingLibrary.Interfaces;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Models; 

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskCompletedContentBuilder : ITaskStatusContentBuilder
    {
        private readonly IAppLogger<TaskCompletedContentBuilder> _logger;

        public TaskCompletedContentBuilder(IAppLogger<TaskCompletedContentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public string taskState => "Completed";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            try
            { 
                var sb = new StringBuilder();
                sb.AppendLine("✅ Completed Tasks:");

                foreach (var task in tasks)
                    sb.AppendLine($" - {task.taskType} {task.taskName} (Due: {task.dueDate:MM/dd/yyyy})");

                sb.AppendLine();
                return sb.ToString();

            }

            catch (ArgumentNullException argEx)
            {
                _logger.LoggError(argEx, "Tasks input was null.");
                throw;
            }
            catch (FormatException fmtEx)
            {
                _logger.LoggError(fmtEx, "Date formatting failed in completed tasks section.");
                throw;
            }
            catch (InvalidOperationException opEx)
            {
                _logger.LoggError(opEx, "Invalid operation occurred while building completed task section.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Unexpected error occurred while building completed task section.");
                throw;
            }
        }
    }
}
