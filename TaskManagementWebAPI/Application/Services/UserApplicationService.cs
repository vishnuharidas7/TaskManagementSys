using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common.ExceptionMessages;
using TaskManagementWebAPI.Domain.Exceptions;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models; 

namespace TaskManagementWebAPI.Application.Services
{
    public class UserApplicationService : IUserApplicationService
    {
        private readonly IRandomPasswordGenerator _randomPasswordGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IAppLogger<UserApplicationService> _logger;
        private readonly IUserCreatedEmailContentBuilder _userEmailContentBuilder;
        private readonly IEmailService _emailService;
        private readonly ITaskManagementRepository _taskManagementRepository;

        public UserApplicationService(
            IRandomPasswordGenerator randomPasswordGenerator, IUserRepository userRepository, IAppLogger<UserApplicationService> logger,
            IUserCreatedEmailContentBuilder userEmailContentBuilder, IEmailService emailService, ITaskManagementRepository taskManagementRepository )
        {
            _randomPasswordGenerator = randomPasswordGenerator ?? throw new ArgumentNullException(nameof(randomPasswordGenerator));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _userEmailContentBuilder = userEmailContentBuilder ?? throw new ArgumentNullException(nameof(userEmailContentBuilder));
            _taskManagementRepository = taskManagementRepository ?? throw new ArgumentNullException(nameof(taskManagementRepository));
        }

        public async Task<bool> CheckUserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(ExceptionMessages.UserExceptions.UsernameRequired);
                //throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            return await _userRepository.CheckUserExists(username);

        }

        public async Task RegisterAsync(RegisterDTO dto)
        {
            try
            {
                //Phone number validation
                if (!IsValidPhoneNumber(dto.PhoneNumber))
                {
                    throw new InvalidPhoneNumberException(ExceptionMessages.Validations.InvalidPhoneNumber);
                }
                //Email Format validation
                if (!IsValidEmailFormat(dto.Email))
                {
                    throw new InvalidEmailFormatException(string.Format(ExceptionMessages.Validations.InvalidEmailFormat , dto.Email));
                    //throw new InvalidEmailFormatException($"Invalid email format - {dto.Email}");
                }
                //Email already exists validation
                var emailexists = await _userRepository.CheckEmailExists(dto.Email);
                if (emailexists)
                {
                    throw new DuplicateEmailException(string.Format(ExceptionMessages.Validations.DuplicateEmail, dto.Email));
                    //throw new DuplicateEmailException($"Email '{dto.Email}' is already registered.");
                }
                //RoleId validation
                var roleid = await _userRepository.CheckRoleExists(dto.RoleId);
                if (!roleid)
                {
                    throw new InvalidRoleIdException(string.Format(ExceptionMessages.Validations.InvalidRole, dto.RoleId));
                   // throw new InvalidRoleIdException($"Invalid RoleId - {dto.RoleId}");
                }
                //Username validation
                string username = dto.UserName;
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException(ExceptionMessages.UserExceptions.UsernameRequired);
                }
                var checkUsernameExist = await _userRepository.CheckUserExists(username);
                if (checkUsernameExist)
                {
                    throw new DuplicateUsernameException(string.Format(ExceptionMessages.UserExceptions.UsernameAlreadyExists, username));
                    //throw new DuplicateUsernameException($"Username {username} Already exists.");
                }



                string randomPswd = _randomPasswordGenerator.GenerateRandomPassword(8);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(randomPswd);
                var user = new Users
                {
                    Name = dto.Name,
                    UserName = dto.UserName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    RoleID = dto.RoleId,
                    gender = dto.Gender,
                    Password = hashedPassword,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    RefreshToken = "",
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
                };

                var userId = await _userRepository.RegisterAsync(user);

                var password = randomPswd;

                var content = _userEmailContentBuilder.BuildContentforNewUser(user, userId, password);
                await _emailService.SendEmailAsync(user.Email, "Welcome to Task Management System – Your Account Details", content);

            }
            catch (Exception ex)
            {
                _logger.LoggWarning("RegisterAsync-Save UserReg failed");
                throw;
            }
        }

        //Added for phone number validation
        private bool IsValidPhoneNumber(string phone)
        {
            return Regex.IsMatch(phone, @"^\d{10}$"); // Example: valid 10-digit number
        }

        //Added for email format validation
        private bool IsValidEmailFormat(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public async Task<Users?> ForgotPassword(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    throw new NotFoundException(ExceptionMessages.UserExceptions.UserNotFound);
                    //throw new NotFoundException("No user exists with the specified email.");
                }
                //return null;

                string newPassword;
                string hashedPassword;

                try
                {
                    newPassword = _randomPasswordGenerator.GenerateRandomPassword(8);
                    hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }
                catch (Exception ex)
                {
                    _logger.LoggWarning("ForgotPassword - Error generating or hashing password: {Message}", ex.Message);
                    throw;
                }

                user.UpdatePassword(hashedPassword); // Domain method

                try
                {
                    await _userRepository.UpdatePasswordAsync(user); // Only updates
                }
                catch (Exception ex)
                {
                    _logger.LoggWarning("ForgotPassword - DB error while updating password: {Message}", ex.Message);
                    throw;
                }

                var content = _userEmailContentBuilder.BuildContentforPasswordReset(user, user.UserId, newPassword);
                await _emailService.SendEmailAsync(user.Email, "Reset Password – Your Account Details", content);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("ForgotPassword - Unexpected error: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<ViewUserDTO>> ViewUsers()
        {
            try
            {
                return await _userRepository.ViewUsers();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task UpdateUser(int id, UpdateUserDTO obj)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    _logger.LoggWarning("UpdateUser - User not found with ID: {UserId}", id);
                    throw new NotFoundException(ExceptionMessages.UserExceptions.UserNotFound);
                    //throw new NotFoundException("User not found");
                }

                // Mapping DTO to entity
                user.UserName = obj.UserName;
                user.Email = obj.Email;
                user.RoleID = obj.RoleID;
                user.Name = obj.Name;
                user.PhoneNumber = obj.PhoneNumber;
                user.gender = obj.Gender;
                user.IsActive = obj.IsActive;

                // Save changes via repo
                await _userRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteUser(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id); //_db.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LoggWarning("DeleteUser-User not found");
                    throw new NotFoundException(ExceptionMessages.UserExceptions.UserNotFound);
                    //throw new NotFoundException("User not found");
                }

                var usertasks = _taskManagementRepository.GetAllTasksByUserId(id);// (t => t.UserId == id);
                if (usertasks.Any())
                {
                    _logger.LoggWarning("DeleteUser - Cannot delete user ID {UserId}, tasks are assigned.", id);
                    throw new InvalidOperationException(ExceptionMessages.UserExceptions.UserCannotBeDeleted);
                    //throw new InvalidOperationException("Cannot delete user. Tasks are assigned to this user.");
                }

                await _userRepository.DeleteUser(user);  
               
            }
            catch (ArgumentNullException argNullEx)
            {
                _logger.LoggWarning("DeleteUser - Argument null: {Message}", argNullEx.Message);
                throw;
            }
        }

        public async Task UpdatePassword(int id, UpdatePasswordDTO obj)
        {
            try
            {
                if (obj.newpswd != obj.confrmNewpswd)
                {
                    throw new ArgumentException(ExceptionMessages.UserExceptions.PasswordNotMatch);
                    //throw new ArgumentException("New password and confirmation do not match.");
                    //throw new Exception("New password and confirmation do not match.");
                }
                var user = await _userRepository.GetUserByIdAsync(id); //_db.User.FindAsync(id);
                if (user == null)
                {
                    _logger.LoggWarning("UpdatePassword-User not found");
                    throw new NotFoundException(ExceptionMessages.UserExceptions.UserNotFound);
                    //throw new NotFoundException("User not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(obj.curpswd, user.Password))
                {
                    _logger.LoggWarning("Current password is incorrect");
                    throw new UnauthorizedAccessException(ExceptionMessages.UserExceptions.InvalidCrendentials);
                   // throw new UnauthorizedAccessException("Current password is incorrect");
                }

                try
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(obj.confrmNewpswd);
                    await _userRepository.SaveAsync(); //_db.SaveChangesAsync();
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

        public async Task<Users> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                return user;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
