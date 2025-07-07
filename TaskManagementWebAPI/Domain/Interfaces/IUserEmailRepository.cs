using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserEmailRepository
    {
        /// <summary>
        /// For fetching user details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Users GetUserById(int id);
        /// <summary>
        /// Fetching user details for sending task completion reminder email
        /// </summary>
        /// <returns></returns>
        IEnumerable<Users> GetAllUsers();
    }
}
