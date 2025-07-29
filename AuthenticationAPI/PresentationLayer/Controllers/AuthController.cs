using AuthenticationAPI.ApplicationLayer.DTOs;
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
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtHelper _jwtHelper;
        private readonly IAppLogger<AuthController> _logger;
        private readonly IAuthRepository _authRepository;
        public AuthController(IAuthService authService,IJwtHelper jwthelper, IAppLogger<AuthController> logger, IAuthRepository authRepository)
        {
            _authService = authService?? throw new ArgumentNullException(nameof(authService), "AuthService cannot be null.");
            _jwtHelper = jwthelper ?? throw new ArgumentNullException(nameof(jwthelper), "Jwthelper cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger),"Jwthelper cannot be null."); // Assign it
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository), "AuthRepository cannot be null.");
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
               _logger.LoggInformation("AuthController-Login end point called");
                _logger.LoggInformation("Login attempt for username: {Username} at {Time}", dto.UserName, DateTime.UtcNow);

                var token = await _authService.LoginAsync(dto);

                if (token == null)
                {
                    _logger.LoggWarning("Login failed for username: {Username}", dto.UserName);
                    return Unauthorized("Invalid username or password");
                }

                //_logger.LogInformation("Login successful for username: {Username}", dto.UserName);
                return Ok(token);
            }
            catch (Exception ex) {
                _logger.LoggWarning("login API failed");
                throw;
            }
           
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

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Refresh([FromBody] TokenResponseDTO tokens)
        {
            try
            {

               // _logger.LogInformation("Attempting to validate and extract principals from refresh token");
                var principal = _authService.GetPrincipalFromExpiredToken(tokens.RefreshToken);
                var useridClaim = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                int userid = 0;
                if (!string.IsNullOrEmpty(useridClaim))
                {
                    userid = int.Parse(useridClaim);
                }
                //var user = _db.User.SingleOrDefault(u => u.UserId == userid);

                var user = await _authRepository.GetUserAsync(userid);
                if (user == null)
                {
                    _logger.LoggWarning("Checking DB-Invalid refresh token");
                    return BadRequest("Invalid refresh token");
                }
               // _logger.LogInformation("Refresh token generated");
                var newAccessToken = _jwtHelper.GenerateAccessToken(user);

                return Ok(new
                {
                    AccessToken = newAccessToken,

                });
            }
          
            catch (Exception ex)
            {
                _logger.LoggWarning("Refresh API failed");
                throw;
            }
           
        }
    }
}
