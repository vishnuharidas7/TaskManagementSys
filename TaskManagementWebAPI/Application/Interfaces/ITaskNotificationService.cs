using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface ITaskNotificationService
    {
        Task SendNotificationAsync(Users user, IEnumerable<Tasks> tasks);
    }
}
