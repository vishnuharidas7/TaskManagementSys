using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using TaskManagement_Project.Repositories;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserRepository _user;

        public RegistrationController(ApplicationDbContext db, UserRepository user)
        {
            
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _user = user;
            
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO dto)
        {
            var exists = await _db.User.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return BadRequest(new { error = "Email already exists." });
            //return BadRequest("Email already exists.");

            //var user = new Users
            //{
            //    UserName = dto.UserName,
            //    Email = dto.Email,
            //    RoleID = dto.RoleId,
            //    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            //    CreatedDate = DateTime.UtcNow,
            //    IsActive = true,
            //    RefreshToken = "",
            //    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            //};

            //_db.User.Add(user);
            //await _db.SaveChangesAsync();

            //return Ok(user);

            await _user.RegisterAsync(dto);
            return Ok(dto);
        }
    }
}
