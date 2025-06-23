using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models; 
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppLogger<UserRepository> _logger;
        private readonly INewUserEmailContentBuilder _userEmailContentBuilder;
        private readonly IEmailService _emailService;

        public UserRepository(ApplicationDbContext db, IAppLogger<UserRepository> logger, INewUserEmailContentBuilder userEmailContentBuilder, IEmailService emailService)
        {
            _db = db;
            _logger = logger;
            _userEmailContentBuilder = userEmailContentBuilder;
            _emailService = emailService;
        }

        public async Task RegisterAsync(RegisterDTO dto)
        {
            try
            {
                var user = new Users
                {
                    Name = dto.Name,
                    UserName = dto.UserName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    RoleID = dto.RoleId,
                    gender = dto.Gender,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    RefreshToken = "",
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
                };

                _db.User.Add(user);
                await _db.SaveChangesAsync();
                var userId = user.UserId;
                var password = dto.Password;

                var content = _userEmailContentBuilder.BuildContent(user, userId, password);
                await _emailService.SendEmailAsync(user.Email, "Welcome to Task Management System – Your Account Details", content);


                // return user;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("RegisterAsync-Save UserReg failed");
                throw;
            }
        }

        public async Task<List<ViewUserDTO>> ViewUsers()
        {
            try
            {
               
                var usersWithRoles = await _db.User
                .Include(u => u.Role)
                .Select(u => new ViewUserDTO
                {
                    Id = u.UserId,
                    Name = u.Name,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Gender = u.gender,
                    RoleId = u.RoleID,
                    RoleName = u.Role.RoleName,
                    Status = u.IsActive
                })
                .ToListAsync();

                return usersWithRoles;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("ViewUsers- View user failed");
                throw;
            }
        }

        public async Task UpdateUser(int id, UpdateUserDTO obj)
        {
            try
            {
                var user = await _db.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LoggWarning("UpdateUser-User not found");
                    throw new Exception("User not found");
                }

                user.UserName = obj.UserName;
                user.Email = obj.Email;
                user.RoleID = obj.RoleID;
                // user.IsActive = obj.IsActive;

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("UpdateUser-Update user failed");
                throw;
            }
        }

        public async Task DeleteUser(int id)
        {
            try
            {
                var user = await _db.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LoggWarning("DeleteUser-User not found");
                    throw new Exception("User not found");
                }
                _db.User.Remove(user);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("DeleteUser-Delete user failed");
                throw;
            }

        }
    }
}
