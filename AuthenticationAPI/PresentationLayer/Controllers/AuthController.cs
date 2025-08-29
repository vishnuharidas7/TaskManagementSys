using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.Common;
using AuthenticationAPI.InfrastructureLayer.Data;
using AuthenticationAPI.InfrastructureLayer.Helpers;
using AuthenticationAPI.Repositories;
using AuthenticationAPI.Services;
using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AuthenticationAPI.Controllers
{
    [Route(AuthAPIEndpoints.Base)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtHelper _jwtHelper;
        private readonly IAppLogger<AuthController> _logger;
        private readonly IAuthRepository _authRepository;
        public AuthController(IAuthService authService,IJwtHelper jwthelper, IAppLogger<AuthController> logger, IAuthRepository authRepository)
        {
            _authService = authService?? throw new ArgumentNullException(nameof(authService));
            _jwtHelper = jwthelper ?? throw new ArgumentNullException(nameof(jwthelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Assign it
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        }


        /// <summary>
        /// Authenticates a user and returns JWT tokens.
        /// </summary>
        /// <param name="dto">Login credentials (username and password)</param>
        /// <returns>JWT tokens (AccessToken and RefreshToken)</returns>
        /// <response code="200">Login successful, tokens returned</response>
        /// <response code="400">Invalid input or null data</response>
        /// <response code="401">Unauthorized - invalid credentials</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(AuthAPIEndpoints.Post.Login)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        { 
               _logger.LoggInformation("AuthController-Login end point called");
                _logger.LoggInformation("Login attempt for username: {Username} at {Time}", dto.UserName, DateTime.UtcNow);

                var token = await _authService.LoginAsync(dto);

                if (token == null)
                {
                    _logger.LoggWarning("Login failed for username: {Username}", dto.UserName);
                    return Unauthorized("Invalid username or password");
                }
                 
                return Ok(token);
            
           
        }


        /// <summary>
        /// API Controller for refreshing the token
        /// </summary>
        /// <param name="tokens">Tokens is Mandatory</param>
        /// <returns>New access token</returns>
        /// <response code="200">Successfully refreshed token</response>
        /// <response code="400">Invalid refresh token or input</response>
        /// <response code="401">Unauthorized - refresh token expired or tampered</response>
        /// <response code="500">Internal server error</response>

        [HttpPost(AuthAPIEndpoints.Post.Refresh)]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Refresh([FromBody] TokenResponseDto tokens)
        { 
                 
                var principal = _authService.GetPrincipalFromExpiredToken(tokens.RefreshToken);
                var useridClaim = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                int userid = 0;
                if (!string.IsNullOrEmpty(useridClaim))
                {
                    userid = int.Parse(useridClaim);
                } 

                var user = await _authRepository.GetUserAsync(userid);
                if (user == null)
                {
                    _logger.LoggWarning("Checking DB-Invalid refresh token");
                    return BadRequest("Invalid refresh token");
                } 
                var newAccessToken = _jwtHelper.GenerateAccessToken(user);

                return Ok(new
                {
                    AccessToken = newAccessToken,

                });
           
           
        }
    }
}
