using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using SendGrid.Helpers.Mail;
using System.Data.Common;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models; 
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppLogger<UserRepository> _logger;
        private readonly IUserCreatedEmailContentBuilder _userEmailContentBuilder;
        private readonly IEmailService _emailService;
        private readonly IRandomPasswordGenerator _randomPasswordGenerator;

        public UserRepository(ApplicationDbContext db, IAppLogger<UserRepository> logger, IUserCreatedEmailContentBuilder userEmailContentBuilder, IEmailService emailService,IRandomPasswordGenerator randomPasswordGenerator)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db), "db cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
            _userEmailContentBuilder = userEmailContentBuilder ?? throw new ArgumentNullException(nameof(userEmailContentBuilder), "userEmailContentBuilder cannot be null.");
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService), "emailService cannot be null.");
            _randomPasswordGenerator = randomPasswordGenerator ?? throw new ArgumentNullException(nameof(randomPasswordGenerator), "randomPasswordGenerator cannot be null.");
        }


        public async Task<bool> CheckUserExists(string username)
        {
            try {
                try
                {
                    return await _db.User.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
                }
                catch (DbException ex)
                {
                    _logger.LoggError(ex, "CheckUserExists - Database access error.");
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("CheckUserExists validation failed");
                throw;
            }
        }

        public async Task<bool> CheckEmailExists(string email)
        {
            try
            {
                try
                {
                    return await _db.User.AnyAsync(u => u.Email == email);
                }
                catch (DbException ex) 
                {
                    _logger.LoggError(ex, "CheckEmailExists - Database access error.");
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("CheckEmailExists - Database access error.");
                throw;
            }
        }

        public async Task<bool> CheckRoleExists(int role)
        {
            try
            {
                try
                {
                    return await _db.Role.AnyAsync(u => u.RoleId == role);
                }
                catch (DbException ex)
                {
                    _logger.LoggError(ex, "CheckEmailExists - Database access error.");
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("CheckEmailExists - Database access error.");
                throw;
            }
        }

        public async Task<Users> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _db.User.FindAsync(userId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "GetUserByIdAsync - Invalid operation while querying users.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "GetUserByIdAsync - Database access error.");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "GetUserByIdAsync - An unexpected error occurred.");
                throw;
            }
        }

        public async Task<Users>GetUserByCreatedBy(int createBy)
        {
            try
            {
                var user = await _db.User.FindAsync(createBy);
                return user;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "GetUserByCreatedBy - Invalid operation while querying users.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "GetUserByCreatedBy - Database access error.");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "GetUserByCreatedBy - An unexpected error occurred.");
                throw;
            }

        }
        public async Task<List<Users>> ListAllUsers()
        {
            try
            {
                var ListAllUsers = await _db.User.ToListAsync();
                return ListAllUsers;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggError(ex, "ListAllUsers - Invalid operation while querying users.");
                throw ex.InnerException;
            }
            catch (DbException ex)
            {
                _logger.LoggError(ex, "ListAllUsers - Database access error.");
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "ListAllUsers - An unexpected error occurred.");
                throw;
            }

        }
        public IEnumerable<Users> GetAllUsers()
        {
            try
            {
                return _db.User.ToList();
            }
            catch (ArgumentNullException argEx)
            {
                throw;
            }
            catch (InvalidOperationException invEx)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> RegisterAsync(Users user)
        {
            try
            {
                try
                {
                    _db.User.Add(user);
                    await _db.SaveChangesAsync();
                    return user.UserId;
                }
                catch (DbException ex)
                {
                    _logger.LoggError(ex, "Register User - Database access error.");
                    throw ex.InnerException;
                }
                 
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
                    Status = u.IsActive,
                    //Password=u.Password
                })
                .ToListAsync();

                return usersWithRoles;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LoggWarning("ViewUsers - Invalid operation: {Message}", ex.Message);
                throw ex;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LoggWarning("ViewUsers - Database update error: {Message}", dbEx.Message);
                throw dbEx;
            }
            catch (TaskCanceledException tcEx)
            {
                _logger.LoggWarning("ViewUsers - Task was cancelled or timed out: {Message}", tcEx.Message);
                throw tcEx;
            }
            catch (SqlException sqlEx)
            {
                _logger.LoggWarning("ViewUsers - SQL error: {Message}", sqlEx.Message);
                throw sqlEx;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("ViewUsers- View user failed");
                throw;
            }
        }


        public async Task<Users?> GetUserByEmailAsync(string email)
        {
            return await _db.User.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdatePasswordAsync(Users user)
        {
            _db.User.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateUser(int id, UpdateUserDTO obj)
        {
            try
            {
                var user = await _db.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LoggWarning("UpdateUser-User not found");
                    throw new NotFoundException("User not found");
                }

                user.UserName = obj.UserName;
                user.Email = obj.Email;
                user.RoleID = obj.RoleID;
                user.Name = obj.Name;
                user.PhoneNumber = obj.PhoneNumber;  
                //user.Password =BCrypt.Net.BCrypt.HashPassword(obj.Password);
                user.gender = obj.Gender;
                user.IsActive = obj.IsActive;
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException concurrencyEx)
                {
                    _logger.LoggWarning("UpdateUser - Concurrency issue while updating user ID {UserId}: {Message}", id, concurrencyEx.Message);
                    throw;
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LoggWarning("UpdateUser - Database error while saving changes for user ID {UserId}: {Message}", id, dbEx.Message);
                    throw;
                }
            }
            catch (ArgumentNullException argNullEx)
            {
                _logger.LoggWarning("UpdateUser - Argument null: {Message}", argNullEx.Message);
                throw;
            }
            catch (InvalidOperationException invEx)
            {
                _logger.LoggWarning("UpdateUser - Invalid operation: {Message}", invEx.Message);
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LoggWarning("UpdateUser - DB update exception: {Message}", dbEx.Message);
                throw;
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
                    throw new NotFoundException("User not found");
                }
                bool hasTasks = await _db.Task.AnyAsync(t => t.UserId == id);
                if (hasTasks)
                {
                    _logger.LoggWarning("DeleteUser - Cannot delete user ID {UserId}, tasks are assigned.", id);
                    throw new InvalidOperationException("Cannot delete user. Tasks are assigned to this user.");
                }
                try
                {
                    _db.User.Remove(user);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException concurrencyEx)
                {
                    _logger.LoggWarning("DeleteUser - Concurrency issue deleting user ID {UserId}: {Message}", id, concurrencyEx.Message);
                    throw;
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LoggWarning("DeleteUser - Database error deleting user ID {UserId}: {Message}", id, dbEx.Message);
                    throw;
                }
            }
            catch (ArgumentNullException argNullEx)
            {
                _logger.LoggWarning("DeleteUser - Argument null: {Message}", argNullEx.Message);
                throw;
            }
            catch (InvalidOperationException invEx)
            {
                _logger.LoggWarning("DeleteUser - Invalid operation: {Message}", invEx.Message);
                throw;
            } 
            catch (Exception ex)
            {
                _logger.LoggWarning("DeleteUser-Delete user failed");
                throw;
            }

        }

        public async Task<ViewUserDTO?> UserListById(int id)
        {
            try
            {
                ViewUserDTO? userWithId;
                try
                {
                    userWithId = await _db.User
                    .Include(u => u.Role)
                    .Where(u => u.UserId == id)
                    .Select(u => new ViewUserDTO()
                    {
                        UserName = u.UserName,
                        Email = u.Email,
                        Name = u.Name,
                        PhoneNumber = u.PhoneNumber,
                        Password = u.Password,
                        Gender = u.gender
                    }).FirstOrDefaultAsync();

                    if(userWithId ==null)
                    {
                        _logger.LoggWarning("UserListById-User not found");
                        throw new NotFoundException("User not found");
                    }
                }
                catch (InvalidOperationException invOpEx)
                {
                    _logger.LoggWarning("UserListById - Invalid operation fetching user ID {UserId}: {Message}", id, invOpEx.Message);
                    throw;
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LoggWarning("UserListById - Database error fetching user ID {UserId}: {Message}", id, dbEx.Message);
                    throw;
                }
                return userWithId; 

            }
            catch (InvalidOperationException invEx)
            {
                _logger.LoggWarning("UserListById - Invalid operation: {Message}", invEx.Message);
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LoggWarning("UserListById - DB update exception: {Message}", dbEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("ViewUserById - fetching user failed");
                throw;
            }
        }

        public async Task UpdatePassword(int id, UpdatePasswordDTO obj)
        {
            try
            {
                if (obj.newpswd != obj.confrmNewpswd)
                {
                    throw new ArgumentException("New password and confirmation do not match.");
                    //throw new Exception("New password and confirmation do not match.");
                }
                var user = await _db.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LoggWarning("UpdatePassword-User not found");
                    throw new NotFoundException("User not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(obj.curpswd,user.Password))
                {
                    _logger.LoggWarning("Current password is incorrect");
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }

                try
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(obj.confrmNewpswd);
                    await _db.SaveChangesAsync();
                }
                catch (CryptographicException cryptoEx)
                {
                    _logger.LoggWarning("UpdatePassword - Cryptographic error while verifying password: {Message}", cryptoEx.Message);
                    throw;
                }


            }
            catch (Exception ex)
            {
                _logger.LoggWarning("UpdatePassword-Update password failed");
                throw;
            }
        }
    }
}
