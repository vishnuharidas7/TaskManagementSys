using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface ITaskEmailRepository
    {
        /// <summary>
        /// For fetching task details for making email body
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<Tasks> GetTasksByUserId(int userId);
    }
}
