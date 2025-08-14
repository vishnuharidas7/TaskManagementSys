using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Users user, IEnumerable<Tasks> tasks);
    }
}
