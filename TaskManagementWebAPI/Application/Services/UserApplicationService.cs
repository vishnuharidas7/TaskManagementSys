using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Exceptions;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Infrastructure.Repositories;

namespace TaskManagementWebAPI.Application.Services
{
    public class UserApplicationService : IUserApplicationService
    {
        private readonly IRandomPasswordGenerator _randomPasswordGenerator;
        private readonly IUserRepository _userRepository;
        private readonly IAppLogger<UserRepository> _logger;
        private readonly IUserCreatedEmailContentBuilder _userEmailContentBuilder;
        private readonly IEmailService _emailService; 

        public UserApplicationService(
            IRandomPasswordGenerator randomPasswordGenerator, IUserRepository userRepository, IAppLogger<UserRepository> logger,
            IUserCreatedEmailContentBuilder userEmailContentBuilder, IEmailService emailService)
        { 
            _randomPasswordGenerator = randomPasswordGenerator ?? throw new ArgumentNullException(nameof(randomPasswordGenerator), "randomPasswordGenerator cannot be null.");
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository),"user repository cannot be null");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService), "emailService cannot be null.");
            _userEmailContentBuilder = userEmailContentBuilder ?? throw new ArgumentNullException(nameof(userEmailContentBuilder), "userEmailContentBuilder cannot be null.");
        }

        public async Task<bool> CheckUserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            return await _userRepository.CheckUserExists(username);
             
        }

        public async Task RegisterAsync(RegisterDTO dto)
        {
            try
            {
                //Phone number validation
                if (!IsValidPhoneNumber(dto.PhoneNumber))
                {
                    throw new InvalidPhoneNumberException($"Invalid phone number - {dto.PhoneNumber}"); 
                }
                //Email Format validation
                if(!IsValidEmailFormat(dto.Email))
                {
                    throw new InvalidEmailFormatException($"Invalid email format - {dto.Email}"); 
                }
                //Email already exists validation
                var emailexists = await _userRepository.CheckEmailExists(dto.Email); 
                if (emailexists)
                {
                    throw new DuplicateEmailException($"Email '{dto.Email}' is already registered.");
                }
                //RoleId validation
                var roleid = await _userRepository.CheckRoleExists(dto.RoleId);
                if (!roleid)
                { 
                    throw new InvalidRoleIdException($"Invalid RoleId - {dto.RoleId}");
                } 
                //Username validation
                string username = dto.UserName;
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Username cannot be null or empty.", nameof(username));
                }
                var checkUsernameExist = await _userRepository.CheckUserExists(username); 
                if (checkUsernameExist)
                {
                    throw new DuplicateUsernameException($"Username {username} Already exists.");
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
                    throw new NotFoundException("No user exists with the specified email.");
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

    }
}
