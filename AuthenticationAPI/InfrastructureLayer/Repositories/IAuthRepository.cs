using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.Models;
namespace AuthenticationAPI.Repositories
{

    public interface IAuthRepository
    {
        /// <summary>
        /// For fetching active users
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<Users?> GetActiveUserAsync(LoginDTO dto);

        /// <summary>
        /// For fetching active user for refresh token
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        Task<Users?> GetUserAsync(int userid);


    }
}
