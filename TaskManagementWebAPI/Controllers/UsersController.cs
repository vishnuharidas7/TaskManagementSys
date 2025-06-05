using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _user;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuthController> _logger;

        public UsersController(IUserRepository user, ApplicationDbContext db, ILogger<AuthController> logger)
        {

            _user = user ?? throw new ArgumentNullException(nameof(user));
            _db = db;
            _logger = logger;
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists([FromQuery] string username)
        {
            try
            {
                var exists = await _db.User.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("check-username API faild");
                throw;
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO dto)
        {
            try
            {
                var exists = await _db.User.AnyAsync(u => u.Email == dto.Email);
                if (exists)
                    return BadRequest(new { error = "Email already exists." });


                await _user.RegisterAsync(dto);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("register API faild");
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("viewusers")]
        public async Task<ActionResult> UserList()
        {
            try
            {
                var allUser = await _user.ViewUsers();
                return Ok(allUser);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("viewusers API faild");
                throw;
            }
        }




        [HttpPut("updateuser/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO obj)
        {
            try
            {
                await _user.UpdateUser(id, obj);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("updateuser API faild");
                throw;
            }
        }

        [HttpDelete("deleteUser/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                await _user.DeleteUser(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("deleteUser API faild");
                throw;
            }

        }
    }
}
