using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task RegisterAsync(RegisterDTO dto);

        Task<List<ViewUserDTO>> ViewUsers();

        Task<Users?> ForgotPassword(string email);

        Task UpdateUser(int id, UpdateUserDTO obj);

        Task DeleteUser(int id);

        Task<ViewUserDTO?> UserListById(int id);

        Task UpdatePassword(int id, UpdatePasswordDTO obj);
    }
}
