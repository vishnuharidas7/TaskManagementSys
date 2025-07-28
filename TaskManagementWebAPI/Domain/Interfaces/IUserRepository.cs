using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// For Registering a new user
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<int> RegisterAsync(Users user);

        /// <summary>
        /// For viewing user details
        /// </summary>
        /// <returns></returns>
        Task<List<ViewUserDTO>> ViewUsers();

        /// <summary>
        /// To reset user password which is forgotten by user and need to login
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
       // Task<Users?> ForgotPassword(string email);

        /// <summary>
        /// For email validation for forgot password functionality. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<Users?> GetUserByEmailAsync(string email);

        /// <summary>
        /// to update paswword for forgot password functionality.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task UpdatePasswordAsync(Users user);

        /// <summary>
        /// For updating user details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task UpdateUser(int id, UpdateUserDTO obj);

        /// <summary>
        /// For deleting a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteUser(int id);

        /// <summary>
        /// View corresponding user details 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ViewUserDTO?> UserListById(int id);

        /// <summary>
        /// For updating User Password by the corresponding user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task UpdatePassword(int id, UpdatePasswordDTO obj);

        /// <summary>
        /// Get users with userId for sending mail
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Users> GetUserByIdAsync(int userId);

        /// <summary>
        /// Fetch all users
        /// </summary>
        /// <returns></returns>
        Task<List<Users>> ListAllUsers();
        /// <summary>
        /// Get user with created by id
        /// </summary>
        /// <param name="createBy"></param>
        /// <returns></returns>
        Task<Users> GetUserByCreatedBy(int createBy);

        /// <summary>
        /// Get all user
        /// </summary>
        /// <returns></returns>
        IEnumerable<Users> GetAllUsers();
    }
}
