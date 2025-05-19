using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement_Project.DTOs;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Models;

namespace TaskManagement_Project.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RegisterAsync(RegisterDTO dto)
        {
          

            var user = new Users
            {
                UserName = dto.UserName,
                Email = dto.Email,
                RoleID = dto.RoleId,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                RefreshToken = "",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            _db.User.Add(user);
            await _db.SaveChangesAsync();

           // return user;
        }

        public async Task<List<ViewUserDTO>> ViewUsers()
        {

            var usersWithRoles = await _db.User
            .Include(u => u.Role)
            .Select(u => new ViewUserDTO
            {
                Id = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                Status = u.IsActive
            })
            .ToListAsync();

            return usersWithRoles;
        }

        public async Task DeleteUser(int id)
        {
            var user = await _db.User.FindAsync(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            _db.User.Remove(user);
            await _db.SaveChangesAsync();

        }
    }
}
