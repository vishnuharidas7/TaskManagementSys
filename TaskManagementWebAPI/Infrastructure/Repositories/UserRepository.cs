using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

        //public async Task<Users?> ForgotPassword(string email)
        //{
        //    try
        //    {
        //        Users? user;

        //        try
        //        {
        //            user = await _db.User.Where(u => u.Email == email).FirstOrDefaultAsync();
        //        }
        //        catch (InvalidOperationException invEx)
        //        {
        //            _logger.LoggWarning("ForgotPassword - Invalid operation while retrieving user: {Message}", invEx.Message);
        //            throw invEx;
        //        }
        //        catch (DbUpdateException dbEx)
        //        {
        //            _logger.LoggWarning("ForgotPassword - Database access error: {Message}", dbEx.Message);
        //            throw dbEx;
        //        }

        //        if (user != null)
        //        {
        //            string newPassword;
        //            string hashedPassword;
        //            try
        //            {
        //                newPassword = _randomPasswordGenerator.GenerateRandomPassword(8);
        //                hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);//_passwordHasher.Hash(newPassword);
        //            }
        //            catch (ArgumentException argEx)
        //            {
        //                _logger.LoggWarning("ForgotPassword - Error generating or hashing password: {Message}", argEx.Message);
        //                throw;
        //            }
        //            catch (CryptographicException cryptoEx)
        //            {
        //                _logger.LoggWarning("ForgotPassword - Cryptographic error while hashing password: {Message}", cryptoEx.Message);
        //                throw;
        //            }

        //            try
        //            {
        //                user.UpdatePassword(hashedPassword); // use domain method to update
        //                await _db.SaveChangesAsync();
        //            }
        //            catch (DbUpdateException dbEx)
        //            {
        //                _logger.LoggWarning("ForgotPassword - Error saving new password to database: {Message}", dbEx.Message);
        //                throw;
        //            }


        //            var content = _userEmailContentBuilder.BuildContentforPasswordReset(user, user.UserId, newPassword);
        //            await _emailService.SendEmailAsync(user.Email, "Reset Password – Your Account Details", content);

        //            return user;
        //        }

        //        else return user;

                
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LoggWarning("ViewUser for password resert - View user failed");
        //        throw;
        //    }
        //}

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
                    throw new Exception("User not found");
                }

                user.UserName = obj.UserName;
                user.Email = obj.Email;
                user.RoleID = obj.RoleID;
                user.Name = obj.Name;
                user.PhoneNumber = obj.PhoneNumber;  
                //user.Password =BCrypt.Net.BCrypt.HashPassword(obj.Password);
                user.gender = obj.Gender;
                // user.IsActive = obj.IsActive;
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
                    throw new Exception("User not found");
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
                    throw new Exception("User not found");
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
