using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface ITaskDueStatusUpdateInternalService
    {
        void UpdateTaskStatus(IEnumerable<Tasks> tasks);
    }
}
