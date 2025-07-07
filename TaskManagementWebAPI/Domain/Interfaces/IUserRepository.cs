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
        Task RegisterAsync(RegisterDTO dto);

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
        Task<Users?> ForgotPassword(string email);
        
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
    }
}
