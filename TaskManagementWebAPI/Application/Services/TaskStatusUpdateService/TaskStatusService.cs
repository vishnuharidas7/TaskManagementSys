using LoggingLibrary.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.TaskStatusUpdateService
{
    public class TaskStatusService
    {
        private readonly IAppLogger<TaskStatusService> _logger;
        public TaskStatusService(IAppLogger<TaskStatusService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
        }
        public void UpdateTaskStatus(IEnumerable<Tasks> tasks)
        {
            var today = DateTime.Today;
            foreach (var task in tasks)
            {
                try
                {
                    var daysUntilDue = (task.dueDate - today).Days;
                    if (daysUntilDue <= 2 && daysUntilDue >= 0 && task.taskStatus != "Completed")
                    {
                        task.UpdateStateToDue();
                    }
                    if (daysUntilDue < 0 && task.taskStatus != "Completed")
                    {
                        task.UpdateStateToOverDue();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LoggError(ex, "Failed to update status for task ID {task.taskId}");
                    throw;
                }
            }
        }
    }
}
