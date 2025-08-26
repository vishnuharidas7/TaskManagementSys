using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SendGrid.Helpers.Errors.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Application.PasswordService;
using TaskManagementWebAPI.Application.Services;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Common.ExceptionMessages;
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
        private readonly Mock<IAppLogger<UserApplicationService>> _mockLogger;
        private readonly Mock<ITaskManagementRepository> _mockTaskManagementRepo;
        private readonly Mock<IUserNotificationService> _mockNotificationService;

        private readonly UserApplicationService _service;

    public UserApplicationServiceTest()
    {
        _mockPasswordGenerator = new Mock<IRandomPasswordGenerator>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<IAppLogger<UserApplicationService>>();
        _mockTaskManagementRepo = new Mock<ITaskManagementRepository>();
        _mockNotificationService = new Mock<IUserNotificationService>();

        _service = new UserApplicationService(
            _mockPasswordGenerator.Object,
            _mockUserRepository.Object,
            _mockLogger.Object,
            _mockTaskManagementRepo.Object,
            _mockNotificationService.Object
        );
    }

    [Fact]
        public async Task CheckUserExists_UserExists_ReturnsTrue()
        {
            // Arrange
            var username = "testuser";
            _mockUserRepository.Setup(r => r.CheckUserExists(username)).ReturnsAsync(true);
            //var service = _service();

            // Act
            var result = await _service.CheckUserExists(username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckUserExists_UserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var username = "nouser";
            _mockUserRepository.Setup(r => r.CheckUserExists(username)).ReturnsAsync(false);
            //var service = CreateService();

            // Act
            var result = await _service.CheckUserExists(username);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CheckUserExists_InvalidUsername_ThrowsArgumentException(string invalidUsername)
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CheckUserExists(invalidUsername));

            Assert.Equal(ExceptionMessages.UserExceptions.UsernameRequired, ex.Message);
        }
        //public async Task CheckUserExists_InvalidUsername_ThrowsArgumentException(string invalidUsername)
        //{
        //    // Arrange
        //   // var service = CreateService();

        //    // Act & Assert
        //    var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CheckUserExists(invalidUsername));
        //    Assert.Equal("Username cannot be null or empty. (Parameter 'username')", ex.Message);
        //}

        // [Fact]
        //public async Task RegisterAsync_ShouldRegisterUser_WhenDataIsValid()
        //{
        //    // Arrange
        //    var dto = new RegisterDTO
        //    {
        //        Name = "John",
        //        UserName = "john123",
        //        Email = "john@example.com",
        //        PhoneNumber = "1234567890",
        //        RoleId = 1,
        //        Gender = "Male"
        //    };

        //    _mockUserRepository.Setup(r => r.CheckEmailExists(dto.Email)).ReturnsAsync(false);
        //    _mockUserRepository.Setup(r => r.CheckRoleExists(dto.RoleId)).ReturnsAsync(true);
        //    _mockUserRepository.Setup(r => r.CheckUserExists(dto.UserName)).ReturnsAsync(false);
        //    _mockPasswordGenerator.Setup(p => p.GenerateRandomPassword(8)).Returns("password");
        //    _mockUserRepository.Setup(r => r.RegisterAsync(It.IsAny<Users>())).ReturnsAsync(1);
        //    //_mockEmailContentBuilder.Setup(e => e.BuildContentforNewUser(It.IsAny<Users>(), 1, "password"))
        //    //                 .Returns("Email content");
        //    //_mockEmailService.Setup(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()))
        //    //                 .Returns(Task.CompletedTask);
        //    _mockNotificationService.Setup(n => n.SendEmailAsync(It.IsAny<Users>(), 1, "password123", "New")).Returns(Task.CompletedTask);


        //    // Act
        //    await _service.RegisterAsync(dto);

        //    // Assert
        //    //    _mockUserRepository.Verify(r => r.RegisterAsync(It.IsAny<Users>()), Times.Once);
        //    //    _mockEmailService.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), "Email content"), Times.Once);
        //    //}
        //    _mockNotificationService.Verify(n => n.SendEmailAsync(It.IsAny<Users>(), 1, "password123", "New"), Times.Once);
        //}
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
            _mockPasswordGenerator.Setup(p => p.GenerateRandomPassword(8)).Returns("password123");
            _mockUserRepository.Setup(r => r.RegisterAsync(It.IsAny<Users>())).ReturnsAsync(1);
            _mockNotificationService.Setup(n => n.SendEmailAsync(It.IsAny<Users>(), 1, "password123", UserStatus.New)).Returns(Task.CompletedTask);

            // Act
            await _service.RegisterAsync(dto);

            // Assert
            _mockUserRepository.Verify(r => r.RegisterAsync(It.IsAny<Users>()), Times.Once);
            _mockNotificationService.Verify(n => n.SendEmailAsync(It.IsAny<Users>(), 1, "password123", UserStatus.New), Times.Once);
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


        //[Fact]
        //public async Task ForgotPassword_ShouldResetPasswordAndSendEmail_WhenUserExists()
        //{
        //    // Arrange
        //    var user = new Users
        //    {
        //        UserId = 1,
        //        Email = "test@example.com",
        //        Password = "oldhashed"
        //    };

        //    _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
        //    _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Returns("newpass");
        //    _mockUserRepository.Setup(r => r.UpdatePasswordAsync(user)).Returns(Task.CompletedTask);
        //    //_mockEmailContentBuilder.Setup(b => b.BuildContentforPasswordReset(user, user.UserId, "newpass"))
        //    //                 .Returns("reset email content");
        //    //_mockEmailService.Setup(e => e.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()))
        //    //                 .Returns(Task.CompletedTask);
        //    _mockNotificationService.Setup(n => n.SendEmailAsync(user, user.UserId, "newPass123", "Forgot")).Returns(Task.CompletedTask);


        //    // Act
        //    var result = await _service.ForgotPassword(user.Email);

        //    // Assert
        //    Assert.Equal(user.Email, result?.Email);
        //    _mockUserRepository.Verify(r => r.UpdatePasswordAsync(user), Times.Once);
        //    //    _mockEmailService.Verify(e => e.SendEmailAsync(user.Email, It.IsAny<string>(), "reset email content"), Times.Once);
        //    //}
        //    _mockNotificationService.Verify(n => n.SendEmailAsync(It.IsAny<Users>(), 1, "password123", "New"), Times.Once);
        //}
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
            _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Returns("newpass123");
            _mockUserRepository.Setup(r => r.UpdatePasswordAsync(user)).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(n => n.SendEmailAsync(user, user.UserId, "newpass123", UserStatus.PasswordReset)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ForgotPassword(user.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
            _mockUserRepository.Verify(r => r.UpdatePasswordAsync(user), Times.Once);
            _mockNotificationService.Verify(n => n.SendEmailAsync(user, user.UserId, "newpass123", UserStatus.PasswordReset), Times.Once);
        }

        //[Fact]
        //public async Task ForgotPassword_ShouldThrowNotFoundException_WhenUserNotFound()
        //{
        //    _mockUserRepository.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
        //                       .ReturnsAsync((Users?)null);

        //    var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ForgotPassword("notfound@example.com"));

        //    Assert.Equal("No user exists with the specified email.", ex.Message);
        //}
        [Fact]
        public async Task ForgotPassword_ShouldThrowNotFoundException_WhenUserNotFound()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                               .ReturnsAsync((Users?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ForgotPassword("notfound@example.com"));

            Assert.Equal(ExceptionMessages.UserExceptions.UserNotFound, ex.Message);
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

        //[Fact]
        //public async Task ForgotPassword_ShouldThrow_WhenEmailSendingFails()
        //{
        //    var user = new Users
        //    {
        //        UserId = 1,
        //        Email = "user@example.com"
        //    };

        //    _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
        //    _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Returns("newpass");
        //    _mockUserRepository.Setup(r => r.UpdatePasswordAsync(user)).Returns(Task.CompletedTask);
        //    //_mockEmailContentBuilder.Setup(b => b.BuildContentforPasswordReset(user, user.UserId, "newpass")).Returns("email content");
        //    //_mockEmailService.Setup(e => e.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("SMTP fail"));
        //    _mockNotificationService.Setup(n => n.SendEmailAsync(user, user.UserId, "newPass123", "Forgot")).ThrowsAsync(new Exception("SMTP Fail")); //Returns(Task.CompletedTask);


        //    var ex = await Assert.ThrowsAsync<Exception>(() => _service.ForgotPassword(user.Email));
        //    _mockLogger.Verify(l => l.LoggWarning("ForgotPassword - Unexpected error: {Message}", "SMTP fail"), Times.Once);
        //}
        [Fact]
        public async Task ForgotPassword_ShouldThrow_WhenEmailSendingFails()
        {
            // Arrange
            var user = new Users
            {
                UserId = 1,
                Email = "user@example.com"
            };

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _mockPasswordGenerator.Setup(g => g.GenerateRandomPassword(8)).Returns("newpass123");
            _mockUserRepository.Setup(r => r.UpdatePasswordAsync(user)).Returns(Task.CompletedTask);
            _mockNotificationService
                .Setup(n => n.SendEmailAsync(user, user.UserId, "newpass123", UserStatus.PasswordReset))
                .ThrowsAsync(new Exception("SMTP fail"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ForgotPassword(user.Email));

            // Verify logger was called with expected message and exception message
            _mockLogger.Verify(
                l => l.LoggWarning("ForgotPassword - Unexpected error: {Message}", "SMTP fail"),
                Times.Once);
        }

        [Fact]
        public async Task ViewUsers_ReturnsListOfViewUserDTO()
        {
            // Arrange
            var expectedUsers = new List<ViewUserDTO>
        {
            new ViewUserDTO
            {
                Id = 1,
                UserName = "john",
                Email = "john@example.com",
                Name = "John Doe",
                RoleId = 1,
                RoleName = "Admin",
                Status = true,
                PhoneNumber = "1234567890",
                Gender = "Male"
            },
            new ViewUserDTO
            {
                Id = 2,
                UserName = "jane",
                Email = "jane@example.com",
                Name = "Jane Doe",
                RoleId = 2,
                RoleName = "User",
                Status = true,
                PhoneNumber = "0987654321",
                Gender = "Female"
            }
        };

            _mockUserRepository
                .Setup(repo => repo.ViewUsers())
                .ReturnsAsync(expectedUsers);

            // Act
            var actualUsers = await _service.ViewUsers();

            // Assert
            Assert.NotNull(actualUsers);
            Assert.Equal(2, actualUsers.Count);
            Assert.Equal("john", actualUsers[0].UserName);
            Assert.Equal("jane", actualUsers[1].UserName);
        }

        [Fact]
        public async Task ViewUsers_RepositoryThrows_ThrowsException()
        {
            _mockUserRepository.Setup(r => r.ViewUsers()).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _service.ViewUsers());
        }

        [Fact]
        public async Task UpdateUser_ValidId_UpdatesUserAndCallsSave()
        {
            // Arrange
            int userId = 1;
            var existingUser = new Users
            {
                UserId = userId,
                UserName = "oldUser",
                Email = "old@example.com",
                RoleID = 1,
                Name = "Old Name",
                PhoneNumber = "0000000000",
                gender = "Male",
                IsActive = false
            };

            var updateDto = new UpdateUserDTO
            {
                UserName = "newUser",
                Email = "new@example.com",
                RoleID = 2,
                Name = "New Name",
                PhoneNumber = "1111111111",
                Gender = "Female",
                IsActive = true
            };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.UpdateUser(userId, updateDto);

            // Assert
            Assert.Equal(updateDto.UserName, existingUser.UserName);
            Assert.Equal(updateDto.Email, existingUser.Email);
            Assert.Equal(updateDto.Name, existingUser.Name);
            Assert.Equal(updateDto.RoleID, existingUser.RoleID);
            Assert.Equal(updateDto.PhoneNumber, existingUser.PhoneNumber);
            Assert.Equal(updateDto.Gender, existingUser.gender);
            Assert.Equal(updateDto.IsActive, existingUser.IsActive);
            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Once);
        }


        //[Fact]
        //public async Task UpdateUser_UserNotFound_ThrowsNotFoundException()
        //{
        //    // Arrange
        //    int userId = 99;
        //    var updateDto = new UpdateUserDTO
        //    {
        //        UserName = "any",
        //        Email = "any@example.com",
        //        RoleID = 1,
        //        Name = "Any",
        //        PhoneNumber = "123",
        //        Gender = "Other",
        //        IsActive = true
        //    };

        //    _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
        //        .ReturnsAsync((Users)null); 

        //    // Act & Assert
        //    await Assert.ThrowsAsync<NotFoundException>(() =>
        //        _service.UpdateUser(userId, updateDto));

        //    _mockUserRepository.Verify(r => r.SaveAsync(), Times.Never);
        //}

        [Fact]
        public async Task UpdateUser_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int userId = 99;
            var updateDto = new UpdateUserDTO
            {
                UserName = "any",
                Email = "any@example.com",
                RoleID = 1,
                Name = "Any",
                PhoneNumber = "123",
                Gender = "Other",
                IsActive = true
            };

            //_mockUserRepository
            //    .Setup(repo => repo.GetUserByIdAsync(userId))
            //    .ReturnsAsync((Users?)null);

            _mockUserRepository
            .Setup<Task<Users?>>(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync((Users?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateUser(userId, updateDto));

            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Never);

            // ✅ Add this to verify logger call
            _mockLogger.Verify(
                x => x.LoggWarning("UpdateUser - User not found with ID: {UserId}", userId),
                Times.Once
            );
        }


        [Fact]
        public async Task UpdateUser_GetUserThrowsException_ThrowsException()
        {
            // Arrange
            int userId = 1;
            var dto = new UpdateUserDTO(); // values don’t matter here

            _mockUserRepository
                .Setup(repo => repo.GetUserByIdAsync(userId))
                .ThrowsAsync(new Exception("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateUser(userId, dto));

            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_SaveAsyncThrowsException_ThrowsException()
        {
            // Arrange
            int userId = 1;
            var user = new Users { UserId = userId };

            var dto = new UpdateUserDTO
            {
                UserName = "Updated",
                Email = "updated@example.com",
                RoleID = 1,
                Name = "Updated Name",
                PhoneNumber = "9999999999",
                Gender = "Female",
                IsActive = true
            };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.SaveAsync()).ThrowsAsync(new Exception("Save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateUser(userId, dto));

            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Once);
        }



        [Fact]
        public async Task DeleteUser_ValidIdWithoutTasks_DeletesSuccessfully()
        {
            // Arrange
            int userId = 1;
            var user = new Users { UserId = userId };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockTaskManagementRepo.Setup(r => r.GetAllTasksByUserId(userId)).Returns(new List<Tasks>());
            _mockUserRepository.Setup(r => r.DeleteUser(user)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteUser(userId);

            // Assert
            _mockUserRepository.Verify(r => r.DeleteUser(user), Times.Once);
        }

        // [Fact]
        //public async Task DeleteUser_UserNotFound_ThrowsNotFoundException()
        //{
        //    // Arrange
        //    int userId = 99;
        //    _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((Users?)null);

        //    // Act & Assert
        //    await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteUser(userId));
        //    _mockUserRepository.Verify(r => r.DeleteUser(It.IsAny<Users>()), Times.Never);
        //}

        public async Task DeleteUser_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int userId = 99;

            _mockUserRepository
                .Setup<Task<Users?>>(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync((Users?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteUser(userId));

            _mockUserRepository.Verify(r => r.DeleteUser(It.IsAny<Users>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUser_UserHasTasks_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 2;
            var user = new Users { UserId = userId };
            var tasks = new List<Tasks> { new Tasks { taskId = 1, UserId = userId } };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockTaskManagementRepo.Setup(r => r.GetAllTasksByUserId(userId)).Returns(tasks);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteUser(userId));
            _mockUserRepository.Verify(r => r.DeleteUser(It.IsAny<Users>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUser_GetUserThrowsArgumentNullException_LogsAndRethrows()
        {
            // Arrange
            int userId = 1;
            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ThrowsAsync(new ArgumentNullException("id"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.DeleteUser(userId));
        }


        [Fact]
        public async Task DeleteUser_DeleteThrowsException_ExceptionIsRethrown()
        {
            // Arrange
            int userId = 3;
            var user = new Users { UserId = userId };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockTaskManagementRepo.Setup(r => r.GetAllTasksByUserId(userId)).Returns(new List<Tasks>());
            _mockUserRepository.Setup(r => r.DeleteUser(user)).ThrowsAsync(new Exception("Delete failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteUser(userId));
        }


        [Fact]
        public async Task UpdatePassword_ValidData_UpdatesPasswordAndSaves()
        {
            int userId = 1;
            var dto = new UpdatePasswordDTO
            {
                curpswd = "oldpass",
                newpswd = "newpass",
                confrmNewpswd = "newpass"
            };

            var user = new Users { UserId = userId, Password = BCrypt.Net.BCrypt.HashPassword("oldpass") };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            await _service.UpdatePassword(userId, dto);

            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Once);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpass", user.Password));
        }

        //[Fact]
        //public async Task UpdatePassword_PasswordMismatch_ThrowsArgumentException()
        //{
        //    var dto = new UpdatePasswordDTO
        //    {
        //        curpswd = "old",
        //        newpswd = "one",
        //        confrmNewpswd = "two"
        //    };

        //    await Assert.ThrowsAsync<ArgumentException>(() =>
        //        _service.UpdatePassword(1, dto));

        //    _mockUserRepository.Verify(r => r.SaveAsync(), Times.Never);
        //}

        [Fact]
        public async Task UpdatePassword_PasswordMismatch_ThrowsArgumentException()
        {
            // Arrange
            var dto = new UpdatePasswordDTO
            {
                curpswd = "old",
                newpswd = "one",
                confrmNewpswd = "two" // mismatch on purpose
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdatePassword(1, dto));

            //Assert.Equal("New password and confirmation do not match", exception.Message); // Optional check if you have a message
            Assert.Equal("New password and confirmation do not match.", exception.Message);
            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdatePassword_UserNotFound_ThrowsNotFoundException()
        {
            var dto = new UpdatePasswordDTO
            {
                curpswd = "pass",
                newpswd = "pass",
                confrmNewpswd = "pass"
            };

            //_mockUserRepository.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync((Users?)null);
           // _mockUserRepository.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((Users?)null);
            _mockUserRepository.Setup(r => r.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((Users?)null!);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdatePassword(1, dto));

            _mockLogger.Verify(l => l.LoggWarning("UpdatePassword-User not found"), Times.Once);
            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdatePassword_InvalidCurrentPassword_ThrowsUnauthorizedAccess()
        {
            var dto = new UpdatePasswordDTO
            {
                curpswd = "wrong",
                newpswd = "new",
                confrmNewpswd = "new"
            };

            var user = new Users
            {
                Password = BCrypt.Net.BCrypt.HashPassword("actual")
            };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdatePassword(1, dto));

            _mockLogger.Verify(l => l.LoggWarning("Current password is incorrect"), Times.Once);
            _mockUserRepository.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdatePassword_CryptoErrorDuringHash_ThrowsCryptoException()
        {
            var dto = new UpdatePasswordDTO
            {
                curpswd = "correct",
                newpswd = "new",
                confrmNewpswd = "new"
            };

            var user = new Users
            {
                Password = BCrypt.Net.BCrypt.HashPassword("correct")
            };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

            // Simulate exception when saving
            _mockUserRepository.Setup(r => r.SaveAsync()).ThrowsAsync(new CryptographicException("Crypto failed"));

            await Assert.ThrowsAsync<CryptographicException>(() =>
                _service.UpdatePassword(1, dto));

            _mockLogger.Verify(l =>
                l.LoggWarning("UpdatePassword - Cryptographic error while verifying password: {Message}", It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePassword_GeneralException_ThrowsExceptionAndLogs()
        {
            var dto = new UpdatePasswordDTO
            {
                curpswd = "pass",
                newpswd = "pass",
                confrmNewpswd = "pass"
            };

            _mockUserRepository.Setup(r => r.GetUserByIdAsync(1)).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdatePassword(1, dto));

            _mockLogger.Verify(l => l.LoggWarning("UpdatePassword-Update password failed"), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ValidId_ReturnsUser()
        {
            var user = new Users { UserId = 1 };
            _mockUserRepository.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

            var result = await _service.GetUserByIdAsync(1);

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task GetUserByIdAsync_RepoThrows_ThrowsException()
        {
            _mockUserRepository.Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
                               .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetUserByIdAsync(1));
        }


    }

}

