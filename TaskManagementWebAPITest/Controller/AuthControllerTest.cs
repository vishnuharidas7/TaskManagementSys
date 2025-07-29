using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Controllers;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPITest.Controller
{
    public class AuthControllerTest
    {
        private readonly AuthController _authController;
        private readonly Mock<IUserAuthRepository> _userAuthRepoMock;
        private readonly Mock<IAppLogger<AuthController>> _loggerMock;
        private readonly Mock<IForgotPasswordHandler> _forgotPasswordHandlerMock;
        public AuthControllerTest() {
            _userAuthRepoMock = new Mock<IUserAuthRepository>();
            _loggerMock = new Mock<IAppLogger<AuthController>>();
            _forgotPasswordHandlerMock = new Mock<IForgotPasswordHandler>();
            _authController = new AuthController(
                
               _userAuthRepoMock.Object,
               _loggerMock.Object,
               _forgotPasswordHandlerMock.Object);
        }

        [Fact]
        public async Task ExternalLogin_ValidCredentials_ReturnsOkWithToken()
        {
            //Arrange
            var dto = new LoginDTO
            {
                UserName = "amal",
                Password = "amal"
            };

            var expectedToken = "sample-Token@123";

            _userAuthRepoMock.Setup(repo=>repo.LoginAsync(dto)).ReturnsAsync(expectedToken);
            
            //Act
            var result= await _authController.ExternalLogin(dto);
            //Assert
            var okResult=Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedToken, okResult.Value);
                
        }
        [Fact]
        public async Task Refresh_ValidTokens_ReturnsOkWithNewToken()
        {
            //Arrange
            var dto = new TokenResponseDTO
            {
                RefreshToken= "Old-Token-sample-Token@123"
            };

            var expectedNewToken = "New-Token-sample-Token@123";

            _userAuthRepoMock.Setup(repo => repo.Refresh(dto)).ReturnsAsync(expectedNewToken);

            //Act
            var result = await _authController.Refresh(dto);
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedNewToken, okResult.Value);

        }

        [Fact]
        public async Task ForgotPassword_ReturnsOk_WhenUserIsFound()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "test@gmailsample.com" };
            var mockUser = new Users(); // or your actual user class

            _forgotPasswordHandlerMock
                .Setup(h => h.HandleAsync(request))
                .ReturnsAsync(mockUser);

            // Act
            var result = await _authController.ForgotPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var jObject = JObject.FromObject(okResult.Value);
            Assert.Equal("New credentials has been sent to the provided Email.", jObject["Message"]);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsNotFound_WhenUserIsNull()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "notfound@gmailsample.com" };

            _forgotPasswordHandlerMock
                .Setup(h => h.HandleAsync(request))
                .ReturnsAsync((Users)null); // Simulate not found

            // Act
            var result = await _authController.ForgotPassword(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var jObject = JObject.FromObject(notFoundResult.Value);
            Assert.Equal("User with provided email does not exist.", jObject["Error"]?.ToString());
        }

    }
}
