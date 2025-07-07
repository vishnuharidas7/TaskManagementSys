using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface INewUserEmailContentBuilder
    {
        /// <summary>
        /// For making new user email content
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userId"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        string BuildContentforNewUser(Users user, int userId, string Password);

        /// <summary>
        /// For resetting user password email
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userId"></param>
        /// <param name="NewPassword"></param>
        /// <returns></returns>
        string BuildContentforPasswordReset(Users user, int userId, string NewPassword);
    }
}
