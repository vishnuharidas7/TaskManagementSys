using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface IUserApplicationService
    {
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
    }
}
