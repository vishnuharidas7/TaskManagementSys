using Login.DTOs;

namespace Login.Services
{
    public interface IAuthService
    {
        Task<Object> LoginAsync(LoginDTO dto);
    }
}
