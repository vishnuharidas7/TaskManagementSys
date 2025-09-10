using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// For checking username already exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<bool> CheckUserExists(string username);

        /// <summary>
        /// For checking email already exists
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> CheckEmailExists(string email);

        /// <summary>
        /// For checking role exists
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<bool>CheckRoleExists(int role);

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
        /// For deleting a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteUser(Users user);

        /// <summary>
        /// View corresponding user details 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ViewUserDTO?> UserListById(int id);

        /// <summary>
        /// Get users with userId for sending mail
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Users?> GetUserByIdAsync(int userId);

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

        /// <summary>
        /// For saving data
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();
    }
}
