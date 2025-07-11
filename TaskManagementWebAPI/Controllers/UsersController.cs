﻿using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _user;
        private readonly IUserApplicationService _userApplicationService;
        private readonly ApplicationDbContext _db;
        private readonly IAppLogger<AuthController> _logger;

        public UsersController(IUserRepository user, ApplicationDbContext db, 
           IUserApplicationService userApplicationService ,IAppLogger<AuthController> logger)
        {

            _user = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");
            _db = db ?? throw new ArgumentNullException(nameof(db), "Db cannot be null.");
            _userApplicationService = userApplicationService ?? throw new ArgumentNullException(nameof(userApplicationService),"User Application service cannot be null");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

        }

        /// <summary>
        /// Validation for username at the time for user registration
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
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
                _logger.LoggWarning("check-username API failed");
                throw;
            }
        }

        /// <summary>
        /// For register a new user to the system
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO dto)
        {
            try
            {
                
                var exists = await _db.User.AnyAsync(u => u.Email == dto.Email);
                if (exists)
                    return BadRequest(new { error = "Email already exists." });


                await _userApplicationService.RegisterAsync(dto);
                _logger.LoggInformation("Registered successfully");
                return Ok(dto);

            }
            catch (Exception ex)
            {
                _logger.LoggWarning("register API failed");
                throw;
            }
        }

        /// <summary>
        /// To list the user details
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("viewusers")]
        public async Task<ActionResult> UserList()
        {
            try
            {
                string authHeader = Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    string token = authHeader.Substring("Bearer ".Length).Trim('"');
                    Console.WriteLine("🔐 Raw Token: " + token);
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                }

                var allUser = await _user.ViewUsers();
                return Ok(allUser);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("viewusers API failed");
                throw;
            }
        }


        /// <summary>
        /// To update user details for the corresponding user id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
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
                _logger.LoggWarning("updateuser API failed");
                throw;
            }
        }

        /// <summary>
        /// For deleting the user details corresponding to the user id from the system
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
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
                _logger.LoggWarning("deleteUser API failed");
                throw;
            }

        }

        /// <summary>
        /// TO view user details correspondig to the user id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("viewusersByid/{id}")]
        public async Task<ActionResult> UserListById(int id)
        {
            try
            {
                var userById = await _user.UserListById(id);
                return Ok(userById);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("viewusers API failed");
                throw;
            }
        }

        /// <summary>
        /// To update user password
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPut("updatePswd/{id}")]
        public async Task<ActionResult>UpdatePassword(int id,UpdatePasswordDTO obj)
        {
            try
            {
                await _user.UpdatePassword(id, obj);
                return Ok(obj);
            }
            catch
            {
                _logger.LoggWarning("updatPassword API failed");
                throw;
            }
        }
    }
}
