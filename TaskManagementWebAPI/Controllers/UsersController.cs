using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common.ExceptionMessages;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Infrastructure.Persistence;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        //private readonly IUserRepository _user;
        private readonly IUserApplicationService _userApplicationService; 
        private readonly IAppLogger<UsersController> _logger;

        public UsersController(//IUserRepository user,
           IUserApplicationService userApplicationService ,IAppLogger<UsersController> logger)
        {

           // _user = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null."); 
            _userApplicationService = userApplicationService ?? throw new ArgumentNullException(nameof(userApplicationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }


        /// <summary>
        /// Checks if a user exists by username.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>True if user exists, otherwise false.</returns>
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(ExceptionMessages.UserExceptions.UsernameRequired);
                //return BadRequest("Username is required.");

            var exists = await _userApplicationService.CheckUserExists(username);
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

            var allUser = await _userApplicationService.ViewUsers(); //_user.ViewUsers();
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
            await _userApplicationService.UpdateUser(id, obj); //_user.UpdateUser(id, obj);
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
            await _userApplicationService.DeleteUser(id); //_user.DeleteUser(id);
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
            var userById = await _userApplicationService.GetUserByIdAsync(id); //_user.UserListById(id);
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

            await _userApplicationService.UpdatePassword(id, obj);
                return Ok(obj);
             
        }
    }
}
