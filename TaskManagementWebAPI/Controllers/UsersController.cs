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
            var exists = await _db.User.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
            return Ok(exists);
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

        [Authorize(Roles = "Admin")]
        [HttpGet("viewusers")]
        public async Task<ActionResult> UserList()
        {
            var allUser = await _user.ViewUsers();
            return Ok(allUser);
        }


        //commented
        //public async Task<List<ViewUserDTO>> ViewUsers()
        //{

        //    var usersWithRoles = await _db.User
        //    .Include(u => u.Role)
        //    .Select(u => new ViewUserDTO
        //    {
        //        UserName = u.UserName,
        //        Email = u.Email,
        //        RoleName = u.Role.RoleName,
        //        Status = u.IsActive
        //    })
        //    .ToListAsync();

        //        return usersWithRoles;
        //}

        [HttpPut("updateuser/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO obj)
        {
            await _user.UpdateUser(id, obj);
            return Ok(obj);
        }

        [HttpDelete("deleteUser/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await _user.DeleteUser(id);
            return Ok();

        }
    }
}
