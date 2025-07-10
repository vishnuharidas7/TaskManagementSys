using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Application.Services.TaskStatusUpdateService;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskStatusUpdateServiceRepository
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
