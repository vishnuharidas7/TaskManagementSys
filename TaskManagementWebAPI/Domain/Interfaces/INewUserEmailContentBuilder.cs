using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface INewUserEmailContentBuilder
    {
        string BuildContent(Users user, int userId, string Password);
    }
}
