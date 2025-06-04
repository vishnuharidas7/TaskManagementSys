using AuthenticationAPI.InfrastructureLayer.Data;
using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.InfrastructureLayer.Helpers;
using AuthenticationAPI.Repositories;
using AuthenticationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthenticationAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _db;
        private readonly IJwtHelper _jwtHelper;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthRepository _authRepository;
        public AuthController(IAuthService authService, ApplicationDbContext db, IJwtHelper jwthelper, ILogger<AuthController> logger, IAuthRepository authRepository)
        {
            _authService = authService;
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _jwtHelper = jwthelper;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Assign it
            _authRepository = authRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
               // _logger.LogInformation("AuthController-Login end point called");
                //_logger.LogInformation("Login attempt for username: {Username} at {Time}", dto.UserName, DateTime.UtcNow);

                var token = await _authService.LoginAsync(dto);

                if (token == null)
                {
                    _logger.LogWarning("Login failed for username: {Username}", dto.UserName);
                    return Unauthorized("Invalid username or password");
                }

                //_logger.LogInformation("Login successful for username: {Username}", dto.UserName);
                return Ok(token);
            }
            catch (Exception ex) {
                _logger.LogWarning("login API faild");
                throw;
            }
           
        }


        [HttpPost("refresh")]
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
                    _logger.LogWarning("Checking DB-Invalid refresh token");
                    return BadRequest("Invalid refresh token");
                }
               // _logger.LogInformation("Refresh token generated");
                var newAccessToken = _jwtHelper.GenerateAccessToken(user);

                return Ok(new
                {
                    AccessToken = newAccessToken,

                });
            }
            catch(Exception ex)
            {
                _logger.LogWarning("Refresh API faild");
                throw;
            }
           
        }


    }
}
