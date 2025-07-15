using LoggingLibrary.Interfaces;
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
        /// <returns>Username already exists</returns>
        /// <response code="200">Checks and return username already taken if any</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden page</response>
        /// <response code="500">Internal server error.</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists([FromQuery] string username)
        { 
            var exists = await _db.User.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
                return Ok(exists);
           
        }

        /// <summary>
        /// For register a new user to the system
        /// </summary>
        /// <param name="dto">The Registeration DTO contains used details for registeration</param>
        /// <returns></returns>
        ///  <response code="200">Returns Successful message if user details added to DB</response>
        ///  <response code="400">Bad request</response>
        ///  <response code="403">Forbidden page</response>
        ///  <response code="500">Internal server error.</response>
        [Authorize(Roles = "Admin,User")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO dto)
        { 
            var exists = await _db.User.AnyAsync(u => u.Email == dto.Email);
                if (exists)
                    return BadRequest(new { error = "Email already exists." });


                await _userApplicationService.RegisterAsync(dto);
                _logger.LoggInformation("Registered successfully");
                return Ok(dto); 
        }

        /// <summary>
        /// To list the user details
        /// </summary>
        /// <returns>User details</returns>
        ///  <response code="200">Fetch all the user details</response>
        ///  <response code="403">Forbidden page</response>
        ///  <response code="500">Internal server error.</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("viewusers")]
        public async Task<ActionResult> UserList()
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


        /// <summary>
        /// To update user details for the corresponding user id
        /// </summary>
        /// <param name="id">Corresponding User Id</param>
        /// <param name="obj">Updated details</param>
        /// <returns>Successfully updated message if no error is occurred</returns>
        ///  <response code="200">Successfully updated user details</response>
        ///  <response code="400">Bad request</response>
        ///  <response code="403">Forbidden page</response>
        ///  <response code="500">Internal server error.</response>
        [Authorize(Roles = "Admin,User")]
        [HttpPut("updateuser/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO obj)
        { 
            await _user.UpdateUser(id, obj);
                return Ok(obj);
             
        }

        /// <summary>
        /// For deleting the user details corresponding to the user id from the system
        /// </summary>
        /// <param name="id">Corresponding userid to be removed from the system</param>
        /// <returns>Successfully removed the user</returns>
        ///  <response code="200">Successfully removed the user</response>
        ///  <response code="400">Bad request</response>
        ///  <response code="403">Forbidden page</response>
        ///  <response code="500">Internal server error.</response>
        [Authorize(Roles = "Admin,User")]
        [HttpDelete("deleteUser/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        { 
            await _user.DeleteUser(id);
                return Ok();
             
        }

        /// <summary>
        /// TO view user details correspondig to the user id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>User Details</returns>
        /// <response code="200">Fetch user details corresponding to the user id.provided</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden page</response>
        /// <response code="500">Internal server error.</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("viewusersByid/{id}")]
        public async Task<ActionResult> UserListById(int id)
        {
            var userById = await _user.UserListById(id);
                return Ok(userById);
             
        }

        /// <summary>
        /// To update user password
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="obj">DTO containing current password, new password and confirm password</param>
        /// <returns>Password Updated Successfully</returns>
        /// <response code="200">Password updated successfully</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden page</response>
        /// <response code="500">Internal server error.</response>
        [Authorize(Roles = "Admin,User")]
        [HttpPut("updatePswd/{id}")]
        public async Task<ActionResult>UpdatePassword(int id,UpdatePasswordDTO obj)
        {
             
                await _user.UpdatePassword(id, obj);
                return Ok(obj);
             
        }
    }
}
