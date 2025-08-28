using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route(AuthAPIEndpoints.Base)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthRepository _user;
        private readonly IForgotPasswordHandler _forgotPasswordHandler;
        public AuthController(IUserAuthRepository user, IForgotPasswordHandler forgotPasswordHandler)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _forgotPasswordHandler = forgotPasswordHandler ?? throw new ArgumentNullException(nameof(forgotPasswordHandler));
        }

        /// <summary>
        /// API Controller for user Authentication
        /// </summary>
        /// <param name="dto">To input Login Credentials provided by the User</param>
        /// <returns>Returns Bearer token</returns>
        /// <response code="200">Login Successful, Returns Bearer Token.</response>
        /// <response code="400">Bad request</response>
        /// <response code="401">Unauthorized – Invalid credentials.</response>
        /// <response code="500">Internal server error.</response>
        // [HttpPost("loginAuth")]
        [HttpPost(AuthAPIEndpoints.Post.Login)]
        public async Task<IActionResult> ExternalLogin([FromBody] LoginDTO dto)
        {
              var token = await _user.LoginAsync(dto);
                return Ok(token);
        }

        /// <summary>
        /// For bearer token refresh to keep the user login
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns>Returns New Tokens</returns>
        //[HttpPost("refresh")]
        [HttpPost(AuthAPIEndpoints.Post.Refresh)]
        public async Task<IActionResult> Refresh([FromBody] TokenResponseDTO tokens)
        {
              var token = await _user.Refresh(tokens);
                return Ok(token);
        }

        /// <summary>
        /// For forgot password request to reset password
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Password updated successfully and send Mail.</returns>
        /// <response code="200">Password updated successfully.</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal server error.</response>
        //[HttpPost("forgot-password")]
        [HttpPost(AuthAPIEndpoints.Post.ForgotPassword)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
                var user = await _forgotPasswordHandler.HandleAsync(request);
                if (user == null)
                {
                    return NotFound(new { Error = "User with provided email does not exist." });
                } 
                return Ok(new { Message = "New credentials has been sent to the provided Email." });
        }

    }
}
