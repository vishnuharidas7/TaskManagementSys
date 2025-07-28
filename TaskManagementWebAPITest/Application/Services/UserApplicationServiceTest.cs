using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SendGrid.Helpers.Errors.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Application.PasswordService;
using TaskManagementWebAPI.Application.Services;
using TaskManagementWebAPI.Domain.Exceptions;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Infrastructure.Repositories;

namespace TaskManagementWebAPITest.Application.Services
{
    public class UserApplicationServiceTest
    {
        private readonly Mock<IRandomPasswordGenerator> _mockPasswordGenerator;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAppLogger<UserRepository>> _mockLogger;
        private readonly Mock<IUserCreatedEmailContentBuilder> _mockEmailContentBuilder;
        private readonly Mock<IEmailService> _mockEmailService; 

        private readonly UserApplicationService _service;

        public UserApplicationServiceTest()
        {
            _mockPasswordGenerator = new Mock<IRandomPasswordGenerator>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<IAppLogger<UserRepository>>();
            _mockEmailContentBuilder = new Mock<IUserCreatedEmailContentBuilder>();
            _mockEmailService = new Mock<IEmailService>(); 

            _service = new UserApplicationService( 
                _mockPasswordGenerator.Object,
                _mockUserRepository.Object,
                _mockLogger.Object,
                _mockEmailContentBuilder.Object,
                _mockEmailService.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegisterUser_WhenDataIsValid()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Name = "John",
                UserName = "john123",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                RoleId = 1,
                Gender = "Male"
            };

            _mockUserRepository.Setup(r => r.CheckEmailExists(dto.Email)).ReturnsAsync(false);
            _mockUserRepository.Setup(r => r.CheckRoleExists(dto.RoleId)).ReturnsAsync(true);
            _mockUserRepository.Setup(r => r.CheckUserExists(dto.UserName)).ReturnsAsync(false);
            _mockPasswordGenerator.Setup(p => p.GenerateRandomPassword(8)).Returns("password");
            _mockUserRepository.Setup(r => r.RegisterAsync(It.IsAny<Users>())).ReturnsAsync(1);
            _mockEmailContentBuilder.Setup(e => e.BuildContentforNewUser(It.IsAny<Users>(), 1, "password"))
                             .Returns("Email content");
            _mockEmailService.Setup(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.CompletedTask);

            // Act
            await _service.RegisterAsync(dto);

            // Assert
            _mockUserRepository.Verify(r => r.RegisterAsync(It.IsAny<Users>()), Times.Once);
            _mockEmailService.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), "Email content"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_InvalidPhoneNumber_ThrowsInvalidPhoneNumberException()
        {
            var dto = new RegisterDTO
            {
                Name = "Test",
                UserName = "test",
                Email = "test@example.com",
                PhoneNumber = "abc",
                RoleId = 1,
                Gender = "Male"
            };

            var ex = await Assert.ThrowsAsync<InvalidPhoneNumberException>(() => _service.RegisterAsync(dto));
            Assert.Contains("Invalid phone number", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowInvalidEmailFormatException_WhenEmailFormatInvalid()
        {
            var dto = new RegisterDTO
            {
                Name = "John",
                UserName = "john123",
                Email = "invalid-email",
                PhoneNumber = "1234567890",
                RoleId = 1,
                Gender = "Male"
            };

            await Assert.ThrowsAsync<InvalidEmailFormatException>(() => _service.RegisterAsync(dto));
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowDuplicateEmailException_WhenEmailAlreadyExists()
        {
            var dto = new RegisterDTO
            {
                Name = "John",
                UserName = "john123",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                RoleId = 1,
                Gender = "Male"
            };

            _mockUserRepository.Setup(r => r.CheckEmailExists(dto.Email)).ReturnsAsync(true);

            await Assert.ThrowsAsync<DuplicateEmailException>(() => _service.RegisterAsync(dto));
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowInvalidRoleIdException_WhenRoleDoesNotExist()
        {
            var dto = new RegisterDTO
            {
                Name = "John",
                UserName = "john123",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                RoleId = 99,
                Gender = "Male"
            };

            _mockUserRepository.Setup(r => r.CheckEmailExists(dto.Email)).ReturnsAsync(false);
            _mockUserRepository.Setup(r => r.CheckRoleExists(dto.RoleId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidRoleIdException>(() => _service.RegisterAsync(dto));
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenUsernameIsEmpty()
        {
            var dto = new RegisterDTO
            {
                Name = "John",
                UserName = "",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                RoleId = 1,
                Gender = "Male"
            };

            _mockUserRepository.Setup(r => r.CheckEmailExists(dto.Email)).ReturnsAsync(false);
            _mockUserRepository.Setup(r => r.CheckRoleExists(dto.RoleId)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(dto));
            Assert.Contains("Username cannot be null or empty", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowDuplicateUsernameException_WhenUsernameExists()
        {
            var dto = new RegisterDTO
            {
                Name = "David",
                UserName = "david",
                Email = "david@example.com",
                PhoneNumber = "1234567890",
                RoleId = 1,
                Gender = "Male"
            };

            _mockUserRepository.Setup(r => r.CheckEmailExists(dto.Email)).ReturnsAsync(false);
            _mockUserRepository.Setup(r => r.CheckRoleExists(dto.RoleId)).ReturnsAsync(true);
            _mockUserRepository.Setup(r => r.CheckUserExists(dto.UserName)).ReturnsAsync(true);

            await Assert.ThrowsAsync<DuplicateUsernameException>(() => _service.RegisterAsync(dto));
        }

        [Fact]
        public async Task RegisterAsync_ShouldLogWarning_WhenExceptionOccurs()
        {
            var dto = new RegisterDTO
            {
                Name = "John",
                UserName = "john123",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                RoleId = 1,
                Gender = "Male"
            };

            _mockUserRepository.Setup(r => r.CheckEmailExists(dto.Email)).ThrowsAsync(new Exception("DB Error"));

            await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(dto));
            _mockLogger.Verify(l => l.LoggWarning("RegisterAsync-Save UserReg failed"), Times.Once);
        }


        [Fact]
        public async Task ForgotPassword_ShouldResetPasswordAndSendEmail_WhenUserExists()
        {
            // Arrange
            var user = new Users
            {
                UserId = 1,
                Email = "test@example.com",
                Password = "oldhashed"
            };

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Returns("newpass");
            _mockUserRepository.Setup(r => r.UpdatePasswordAsync(user)).Returns(Task.CompletedTask);
            _mockEmailContentBuilder.Setup(b => b.BuildContentforPasswordReset(user, user.UserId, "newpass"))
                             .Returns("reset email content");
            _mockEmailService.Setup(e => e.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ForgotPassword(user.Email);

            // Assert
            Assert.Equal(user.Email, result?.Email);
            _mockUserRepository.Verify(r => r.UpdatePasswordAsync(user), Times.Once);
            _mockEmailService.Verify(e => e.SendEmailAsync(user.Email, It.IsAny<string>(), "reset email content"), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ShouldThrowNotFoundException_WhenUserNotFound()
        {
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                               .ReturnsAsync((Users?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ForgotPassword("notfound@example.com"));

            Assert.Equal("No user exists with the specified email.", ex.Message);
        }

        [Fact]
        public async Task ForgotPassword_ShouldThrowException_WhenPasswordGenerationFails()
        {
            var user = new Users { UserId = 1, Email = "user@example.com" };
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Throws(new Exception("Gen fail"));

            await Assert.ThrowsAsync<Exception>(() => _service.ForgotPassword(user.Email));
            _mockLogger.Verify(l => l.LoggWarning("ForgotPassword - Error generating or hashing password: {Message}", "Gen fail"), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ShouldThrowException_WhenUpdatePasswordFails()
        {
            var user = new Users { UserId = 1, Email = "user@example.com" };
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Returns("newpass");
            _mockUserRepository.Setup(r => r.UpdatePasswordAsync(user)).Throws(new Exception("DB failure"));

            await Assert.ThrowsAsync<Exception>(() => _service.ForgotPassword(user.Email));
            _mockLogger.Verify(l => l.LoggWarning("ForgotPassword - DB error while updating password: {Message}", "DB failure"), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ShouldThrow_WhenEmailSendingFails()
        {
            var user = new Users
            {
                UserId = 1,
                Email = "user@example.com"
            };

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Returns("newpass");
            _mockUserRepository.Setup(r => r.UpdatePasswordAsync(user)).Returns(Task.CompletedTask);
            _mockEmailContentBuilder.Setup(b => b.BuildContentforPasswordReset(user, user.UserId, "newpass")).Returns("email content");
            _mockEmailService.Setup(e => e.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("SMTP fail"));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ForgotPassword(user.Email));
            _mockLogger.Verify(l => l.LoggWarning("ForgotPassword - Unexpected error: {Message}", "SMTP fail"), Times.Once);
        }

    }
}
