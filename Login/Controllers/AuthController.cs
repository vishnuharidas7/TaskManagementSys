using Login.Configurations;
using Login.Data;
using Login.DTOs;
using Login.Helpers;
using Login.Models;
using Login.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Login.Controllers
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

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO dto)
        {
            var exists = await _db.User.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return BadRequest("Email already exists.");

            var user = new Users
            {
                UserName = dto.UserName,
                Email = dto.Email,
                RoleID = dto.RoleId,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                RefreshToken = "", 
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            _db.User.Add(user);
            await _db.SaveChangesAsync();

            return Ok(user);
        }

    }
}
