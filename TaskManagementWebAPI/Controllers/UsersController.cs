using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TaskManagement_Project.DTOs;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Models;
using TaskManagementWebAPI.Repositories;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _user;
        private readonly ApplicationDbContext _db;

        public UsersController(IUserRepository user, ApplicationDbContext db)
        {

            _user = user ?? throw new ArgumentNullException(nameof(user));
            _db = db;
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists([FromQuery] string username)
        {
            try
            {
                var exists = await _db.User.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
                return Ok(exists);
            }
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
                throw;
            }

        }
    }
}
