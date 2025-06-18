using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskStatusRepository
    {
        IEnumerable<Tasks> GetAllTasks();
        void SaveAllTasks();
    }
}
