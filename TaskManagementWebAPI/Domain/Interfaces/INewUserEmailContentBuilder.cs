using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface INewUserEmailContentBuilder
    {
        string BuildContentforNewUser(Users user, int userId, string Password);

        string BuildContentforPasswordReset(Users user, int userId, string NewPassword);
    }
}
