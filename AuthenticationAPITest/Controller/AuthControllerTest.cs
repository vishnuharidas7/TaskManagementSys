using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.Controllers;
using AuthenticationAPI.InfrastructureLayer.Helpers;
using AuthenticationAPI.Models;
using AuthenticationAPI.Repositories;
using AuthenticationAPI.Services;
using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace AuthenticationAPITest.Controller
{
    public class AuthControllerTest
    {
        private readonly AuthController _authController;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IJwtHelper> _jwtHelperMock;
        private readonly Mock<IAppLogger<AuthController>> _loggerMock;
        private readonly Mock<IAuthRepository> _authRepositoryMock;
        public AuthControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _jwtHelperMock=new Mock<IJwtHelper>();
            _loggerMock=new Mock<IAppLogger<AuthController>>();
            _authRepositoryMock=new Mock<IAuthRepository>();
            _authController = new AuthController(_authServiceMock.Object,
                _jwtHelperMock.Object,
                _loggerMock.Object,
                _authRepositoryMock.Object);
        }

        [Fact]
        public async Task Login_ReturnsOk_Token()
        {
            //arrange
            var dto = new LoginDTO {UserName="amal",Password="amal" };
            var expectedToken = "Return-Successful-Token";

            _authServiceMock.Setup(res=>res.LoginAsync(dto)).ReturnsAsync(expectedToken);

            //act
            var result=await _authController.Login(dto);

            //assert
            var okResult=Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedToken, okResult.Value);

        }
        [Fact]
        
        public async Task Login_ReturnsUnauthorized_WhenTokenIsNull()
        {
            // Arrange
            var dto = new LoginDTO { UserName = "amal", Password = "amal" };
            _authServiceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync((string)null);

            // Act
            var result = await _authController.Login(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid username or password", unauthorizedResult.Value);
        }
        [Fact]
        public async Task Login_ThrowsException_AndIsCaught()
        {
            // Arrange
            var dto = new LoginDTO { UserName = "amal", Password = "amal" };
            _authServiceMock.Setup(s => s.LoginAsync(dto)).ThrowsAsync(new Exception("Service failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authController.Login(dto));
        }

        ////Refresh()
        [Fact]
        public async Task Refresh_ReturnOk_WithNewAccessToken()
        {
            //arrange
            var tokens = new TokenResponseDTO { RefreshToken = "Old-Refresh-Token" };
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "123") };
            var identity = new ClaimsIdentity(claims, "test");
            var principal= new ClaimsPrincipal(identity);

            var OldUser = new AuthenticationAPI.Models.Users { UserId = 123, UserName = "amal" };
            var accessToken = "new-access-token";
            _authServiceMock.Setup(res=>res.GetPrincipalFromExpiredToken(tokens.RefreshToken)).Returns(principal);
            _authRepositoryMock.Setup(res => res.GetUserAsync(123)).ReturnsAsync(OldUser);
            _jwtHelperMock.Setup(res=>res.GenerateAccessToken(OldUser)).Returns(accessToken);

            //act
            var result= await _authController.Refresh(tokens);    

            //Assert
            var okResult=Assert.IsType<OkObjectResult>(result);
            var returnedObject = okResult.Value;
            var accessTokenproperty = returnedObject.GetType().GetProperty("AccessToken");
            var actualAccessToken = accessTokenproperty.GetValue(returnedObject).ToString();
            Assert.Equal(accessToken, actualAccessToken);
        }
        [Fact]
        public async Task Refresh_ReturnsBadRequest_WhenUserNotFound()
        {
            // Arrange
            var tokens = new TokenResponseDTO { RefreshToken = "invalid-token" };
            var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, "999")
             };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);

            _authServiceMock.Setup(s => s.GetPrincipalFromExpiredToken(tokens.RefreshToken))
                .Returns(principal);

            _authRepositoryMock.Setup(r => r.GetUserAsync(999))
                .ReturnsAsync((AuthenticationAPI.Models.Users)null);

            // Act
            var result = await _authController.Refresh(tokens);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid refresh token", badRequest.Value);
        }

        [Fact]
        public async Task Refresh_ThrowsException_OnUnexpectedFailure()
        {
            // Arrange
            var tokens = new TokenResponseDTO { RefreshToken = "any" };
            _authServiceMock.Setup(s => s.GetPrincipalFromExpiredToken(tokens.RefreshToken))
                .Throws(new Exception("Unexpected failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authController.Refresh(tokens));
        }
    }
}
