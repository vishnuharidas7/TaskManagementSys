using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Controllers;
using TaskManagementWebAPI.Domain.Exceptions;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPITest.Controller
{
    public class UsersControllerTests
    {
        private readonly UsersController _controller; 
        private readonly Mock<IUserApplicationService> _mockUserAppService;
        private readonly Mock<IAppLogger<UsersController>> _mockLogger;

        public UsersControllerTests()
        { 
            _mockUserAppService = new Mock<IUserApplicationService>();
            _mockLogger = new Mock<IAppLogger<UsersController>>();

            _controller = new UsersController( 
                _mockUserAppService.Object,
                _mockLogger.Object
            );
        }

        /// <summary>
        /// Username already exists validation
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CheckUserExists_ValidUsername_ReturnsOkTrue()
        {
            // Arrange
            var username = "david";
            _mockUserAppService.Setup(s => s.CheckUserExists(username)).ReturnsAsync(true);

            // Act
            var result = await _controller.CheckUserExists(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task CheckUserExists_ValidUsername_ReturnsOkFalse()
        {
            var username = "nonexistent";
            _mockUserAppService.Setup(s => s.CheckUserExists(username)).ReturnsAsync(false);

            var result = await _controller.CheckUserExists(username);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value);
        }


        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CheckUserExists_InvalidUsername_ReturnsBadRequest(string input)
        {
            var result = await _controller.CheckUserExists(input);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username is required.", badRequestResult.Value);
        }

        /// <summary>
        /// User registration
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterAsync_ValidDto_ReturnsOk()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Name = "John Doe",
                UserName = "johndoe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                RoleId = 1,
                Gender = "Male"
            };

            // Setup userApplicationService to do nothing (assume success)
            _mockUserAppService
                .Setup(x => x.RegisterAsync(It.IsAny<RegisterDTO>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RegisterAsync(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(registerDto, okResult.Value); // returns the same DTO
        }


        [Fact]
        public async Task UserList_ReturnsOkWithViewUserDTOList()
        {
            // Arrange
            var fakeUsers = new List<ViewUserDTO>
             {
                new ViewUserDTO
                {
                    Id = 1,
                    UserName = "john",
                    Email = "john@example.com",
                    RoleId = 1,
                    RoleName = "Admin",
                    Status = true,
                    Name = "John Doe",
                    PhoneNumber = "1234567890",
                    Gender = "Male",
                    Password = "hashedpassword"
                },
                new ViewUserDTO
                {
                    Id = 2,
                    UserName = "jane",
                    Email = "jane@example.com",
                    RoleId = 2,
                    RoleName = "User",
                    Status = true,
                    Name = "Jane Smith",
                    PhoneNumber = "0987654321",
                    Gender = "Female",
                    Password = "hashedpassword"
                }
            };

            _mockUserAppService.Setup(repo => repo.ViewUsers())
                .ReturnsAsync(fakeUsers);



            // Act
            var result = await _controller.UserList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<List<ViewUserDTO>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count);
            Assert.Equal("john", returnedUsers[0].UserName);
            Assert.Equal("jane", returnedUsers[1].UserName);
        }

        [Fact]
        public async Task UpdateUser_ValidInput_ReturnsOkWithDto()
        {
            // Arrange
            int userId = 1;
            var updateDto = new UpdateUserDTO
            {
                Name = "Updated Name",
                Email = "updated@example.com",
                PhoneNumber = "1234567890",
                Gender = "Male"
            };

            _mockUserAppService
                .Setup(repo => repo.UpdateUser(userId, updateDto))
                .Returns(Task.CompletedTask); // assuming it returns void

            // Act
            var result = await _controller.UpdateUser(userId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updateDto, okResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ValidId_ReturnsOk()
        {
            // Arrange
            int userId = 1;
            _mockUserAppService.Setup(repo => repo.DeleteUser(userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockUserAppService.Verify(repo => repo.DeleteUser(userId), Times.Once);
        }

        [Fact]
        public async Task UserListById_ValidId_ReturnsOkWithUser()
        {
            // Arrange
            int userId = 1;
            var fakeUser = new Users
            {
                UserId = userId,
                UserName = "john",
                Email = "john@example.com",
                RoleID = 1,
                //Role = "Admin",
                IsActive = true,
                Name = "John Doe",
                PhoneNumber = "1234567890",
                gender = "Male",
                Password = "hashedpassword"
            };

            _mockUserAppService.Setup(repo => repo.GetUserByIdAsync(userId))
                         .ReturnsAsync(fakeUser);

            // Act
            var result = await _controller.UserListById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<Users>(okResult.Value);
            Assert.Equal(userId, returnedUser.UserId);
            Assert.Equal("john", returnedUser.UserName);
        }

        [Fact]
        public async Task UpdatePassword_ValidInput_ReturnsOkWithDto()
        {
            // Arrange
            int userId = 1;
            var updatePasswordDto = new UpdatePasswordDTO
            {
                id = userId,
                curpswd = "oldPassword123",
                newpswd = "newPassword456",
                confrmNewpswd = "newPassword456"
            };

            _mockUserAppService.Setup(r => r.UpdatePassword(userId, updatePasswordDto))
                         .Returns(Task.CompletedTask); // since method returns Task

            // Act
            var result = await _controller.UpdatePassword(userId, updatePasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<UpdatePasswordDTO>(okResult.Value);
            Assert.Equal(userId, returnedDto.id);
            Assert.Equal("newPassword456", returnedDto.newpswd);
        }
    }
}