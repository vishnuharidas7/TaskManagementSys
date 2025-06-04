using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task RegisterAsync(RegisterDTO dto);

        Task<List<ViewUserDTO>> ViewUsers();

        Task UpdateUser(int id, UpdateUserDTO obj);

        Task DeleteUser(int id);
    }
}
