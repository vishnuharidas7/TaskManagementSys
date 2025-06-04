using AuthenticationAPI.ApplicationLayer.DTOs;
using System.Security.Claims;

namespace AuthenticationAPI.Services
{
    public interface IAuthService
    {
        Task<Object> LoginAsync(LoginDTO dto);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
