using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface IUserNotificationService
    {
        Task SendEmailAsync(Users user, int userId, string Password, UserEnums status);
    }
}
