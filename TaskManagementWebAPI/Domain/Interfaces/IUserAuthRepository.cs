using Microsoft.AspNetCore.Mvc; 
using TaskManagementWebAPI.Application.DTOs;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserAuthRepository
    {
        /// <summary>
        /// For User Authentication
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<string> LoginAsync(LoginDTO dto);
        /// <summary>
        /// For refreshing bearer token
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        Task<string> Refresh([FromBody] TokenResponseDTO tokens);
    }
}
