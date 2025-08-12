using LoggingLibrary.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.TaskStatusUpdateService
{
    public class TaskDueStatusUpdateService
    {
        private readonly IAppLogger<TaskDueStatusUpdateService> _logger;
        public TaskDueStatusUpdateService(IAppLogger<TaskDueStatusUpdateService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
