using AuthenticationAPI.Configurations;
using AuthenticationAPI.Data;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using AuthenticationAPI.Models;
using AuthenticationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

        public AuthController(IAuthService authService, ApplicationDbContext db, IOptions<JwtSettings> jwtSettings)
        {
            _authService = authService;
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _jwtHelper = new JwtHelper(jwtSettings);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(token);
        }

        // [Authorize]
        //[HttpGet("me")]
        //public IActionResult GetSomething()
        //{
        //    var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

        //    if (authHeader != null && authHeader.StartsWith("Bearer "))
        //    {
        //        var tokenStr = authHeader.Substring("Bearer ".Length).Trim();

        //        var handler = new JwtSecurityTokenHandler();
        //        var jwtToken = handler.ReadJwtToken(tokenStr);

        //        var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        //        if (userId == null)
        //            return Unauthorized("User ID not found in token");

        //        var parsedUserId = int.Parse(userId);
        //        var user = _db.User.FirstOrDefault(u => u.UserId == parsedUserId);
        //        if (user == null)
        //            return NotFound("User not found");

        //        return Ok(new
        //        {
        //            user.UserId,
        //            user.UserName,
        //            user.Email,
        //            user.RefreshTokenExpiryTime
        //        });
        //    }

        //    return Unauthorized();
        //}

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenResponseDTO tokens)
        {
            var principal = _authService.GetPrincipalFromExpiredToken(tokens.RefreshToken);
            var useridClaim = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            int userid = 0;
            if (!string.IsNullOrEmpty(useridClaim))
            {
                userid = int.Parse(useridClaim);
            }
            //var user = _db.User.SingleOrDefault(u => u.UserId == userid);
            var user = await _db.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userid);
            //if (user == null || user.RefreshToken != tokens.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            //{
            //    return BadRequest("Invalid refresh token");
            //}
            //if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            //{
            //    return BadRequest("Invalid refresh token");
            //}
            var newAccessToken = _jwtHelper.GenerateAccessToken(user);
            //var newRefreshToken = _jwtHelper.GenerateRefreshToken(user);

            // user.RefreshToken = newRefreshToken;
            //user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
           // _db.SaveChanges();

            return Ok(new
            {
                AccessToken = newAccessToken,
                //RefreshToken = tokens.RefreshToken  // Send back only the access token ideally

            });
        }


    }
}
