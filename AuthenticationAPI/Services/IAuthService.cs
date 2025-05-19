using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Services
{
    public interface IAuthService
    {
        Task<Object> LoginAsync(LoginDTO dto);
    }
}
