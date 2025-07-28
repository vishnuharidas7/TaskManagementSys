using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface IUserApplicationService
    {
        /// <summary>
        /// For checking username already exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<bool> CheckUserExists(string username);

        /// <summary>
        /// For Registering a new user
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task RegisterAsync(RegisterDTO dto);

        /// <summary>
        /// For updating user password for forgot password functionality
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<Users?> ForgotPassword(string email);

        /// <summary>
        /// For fetching user details.
        /// </summary>
        /// <returns></returns>
        Task<List<ViewUserDTO>> ViewUsers();

        /// <summary>
        /// For updating user details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task UpdateUser(int id, UpdateUserDTO obj);

        /// <summary>
        /// For deleting the user details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteUser(int id);

        /// <summary>
        /// For updating User Password by the corresponding user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task UpdatePassword(int id, UpdatePasswordDTO obj);

        /// <summary>
        /// Get users corresponding to the userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Users> GetUserByIdAsync(int userId);
    }
}
