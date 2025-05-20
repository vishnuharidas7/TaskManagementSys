using TaskManagement_Project.DTOs;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Models;

namespace TaskManagementWebAPI.Repositories
{
    public interface IUserRepository
    {
        Task RegisterAsync(RegisterDTO dto);

        Task<List<ViewUserDTO>> ViewUsers();

        Task UpdateUser(int id, UpdateUserDTO obj);

        Task DeleteUser(int id);
    }
}
