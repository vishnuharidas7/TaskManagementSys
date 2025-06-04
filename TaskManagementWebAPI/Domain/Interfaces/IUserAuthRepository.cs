using Microsoft.AspNetCore.Mvc; 
using TaskManagementWebAPI.Application.DTOs;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserAuthRepository
    {
        Task<string> LoginAsync(LoginDTO dto);
        Task<string> Refresh([FromBody] TokenResponseDTO tokens);
    }
}
