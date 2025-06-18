using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService
{
    public class TaskStatusService
    {
        public void UpdateTaskStatus(IEnumerable<Tasks> tasks)
        {
            var today = DateTime.Today;
            foreach (var task in tasks)
            {
                var daysUntilDue = (task.dueDate - today).Days;
                if (daysUntilDue <= 2 && daysUntilDue >= 0)
                {
                    task.UpdateStatusToOnDue();
                }
            }
        }
    }
}
