using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskEmailRepository
    {
        IEnumerable<Tasks> GetTasksByUserId(int userId);
    }
}
