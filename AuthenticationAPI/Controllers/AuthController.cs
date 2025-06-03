using AuthenticationAPI.Configurations;
using AuthenticationAPI.Data;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using AuthenticationAPI.Models;
using AuthenticationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController:ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _db;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ApplicationDbContext db, IOptions<JwtSettings> jwtSettings, ILogger<AuthController> logger)
        {
            _authService = authService;
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _jwtHelper = new JwtHelper(jwtSettings);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Assign it
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            _logger.LogInformation("AuthController-Login API");
            _logger.LogInformation("Login attempt for username: {Username} at {Time}", dto.UserName, DateTime.UtcNow);

            var token = await _authService.LoginAsync(dto);

            if (token == null)
            {
                _logger.LogWarning("Login failed for username: {Username}", dto.UserName);
                return Unauthorized("Invalid username or password");
            }

            _logger.LogInformation("Login successful for username: {Username}", dto.UserName);
            return Ok(token);
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenResponseDTO tokens)
        {
            _logger.LogInformation("AuthController-refresh API");
            var principal = _authService.GetPrincipalFromExpiredToken(tokens.RefreshToken);
            var useridClaim = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            int userid = 0;
            if (!string.IsNullOrEmpty(useridClaim))
            {
                userid = int.Parse(useridClaim);
            }
            //var user = _db.User.SingleOrDefault(u => u.UserId == userid);
            var user = await _db.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userid);
            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token");
                return BadRequest("Invalid refresh token");
            }
            _logger.LogInformation("Refresh token generated");
            var newAccessToken = _jwtHelper.GenerateAccessToken(user);

            return Ok(new
            {
                AccessToken = newAccessToken,

            });
        }


    }
}
