using AuthenticationAPI.ApplicationLayer.DTOs;
using System.Security.Claims;

namespace AuthenticationAPI.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// For User's Login
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<Object> LoginAsync(LoginDto dto);
        /// <summary>
        /// Extract the user's identity(claims) from an expired JWT token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
