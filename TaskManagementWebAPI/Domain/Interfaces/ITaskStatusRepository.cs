using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskStatusRepository
    {
        /// <summary>
        /// To fetch all task details to update task state
        /// </summary>
        /// <returns></returns>
        IEnumerable<Tasks> GetAllTasks();
        /// <summary>
        /// For saving the task details.
        /// </summary>
        void SaveAllTasks();
    }
}
