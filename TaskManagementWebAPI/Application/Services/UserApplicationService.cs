using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Security.Cryptography;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
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
        private readonly INewUserEmailContentBuilder _userEmailContentBuilder;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _db;

        public UserApplicationService(ApplicationDbContext db,IRandomPasswordGenerator randomPasswordGenerator, IUserRepository userRepository, IAppLogger<UserRepository> logger,
            INewUserEmailContentBuilder userEmailContentBuilder, IEmailService emailService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db), "db cannot be null.");
            _randomPasswordGenerator = randomPasswordGenerator ?? throw new ArgumentNullException(nameof(randomPasswordGenerator), "randomPasswordGenerator cannot be null.");
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository),"user repository cannot be null");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService), "emailService cannot be null.");
            _userEmailContentBuilder = userEmailContentBuilder ?? throw new ArgumentNullException(nameof(userEmailContentBuilder), "userEmailContentBuilder cannot be null.");
        }
        public async Task RegisterAsync(RegisterDTO dto)
        {
            try
            {
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


        public async Task<Users?> ForgotPassword(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user == null)
                    return null;

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
