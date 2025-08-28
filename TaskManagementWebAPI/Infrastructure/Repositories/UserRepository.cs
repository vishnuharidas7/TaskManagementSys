using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using SendGrid.Helpers.Errors.Model;
using SendGrid.Helpers.Mail;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common.ExceptionMessages;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models; 
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IAppLogger<UserRepository> _logger; 

        public UserRepository(ApplicationDbContext db, IAppLogger<UserRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 
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

        public async Task<Users?> GetUserByIdAsync(int userId)
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

      
        public async Task DeleteUser(Users  user)
        {
            try
            {
                
                try
                {
                    _db.User.Remove(user);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException concurrencyEx)
                {
                    _logger.LoggWarning("DeleteUser - Concurrency issue deleting user ID {UserId}: {Message}", user.UserId, concurrencyEx.Message);
                    throw;
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LoggWarning("DeleteUser - Database error deleting user ID {UserId}: {Message}", user.UserId, dbEx.Message);
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
                        throw new NotFoundException(ExceptionMessages.UserAuthExceptions.UserNotFound);
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

        

        public async Task SaveAsync()
        { 
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            { 
                _logger.LoggWarning("SaveAsync - Concurrency conflict occurred while saving changes.");
                throw;
            }
            catch (DbUpdateException ex)
            { 
                _logger.LoggWarning("SaveAsync - Database update error occurred.");
                throw;
            }
            catch (ValidationException ex)
            { 
                _logger.LoggWarning("SaveAsync - Entity validation failed.");
                throw;
            }
            catch (OperationCanceledException ex)
            { 
                _logger.LoggWarning("SaveAsync - Operation was cancelled.");
                throw;
            }
            catch (ObjectDisposedException ex)
            { 
                _logger.LoggWarning( "SaveAsync - The DbContext was already disposed.");
                throw;
            }
            catch (InvalidOperationException ex)
            { 
                _logger.LoggWarning( "SaveAsync - Invalid operation during database save.");
                throw;
            }
            catch (Exception ex)
            { 
                _logger.LoggError(ex, "SaveAsync - An unexpected error occurred.");
                throw;
            }

        }
    }
}
