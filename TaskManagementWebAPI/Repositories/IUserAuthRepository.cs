using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.DTOs;

namespace TaskManagementWebAPI.Repositories
{
    public interface IUserAuthRepository
    {
        Task<string> LoginAsync(LoginDTO dto);
        Task<string> Refresh([FromBody] TokenResponseDTO tokens);
    }
}
