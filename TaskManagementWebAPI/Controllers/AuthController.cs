using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthRepository _user;
        private readonly IAppLogger<AuthController> _logger;
        private readonly IForgotPasswordHandler _forgotPasswordHandler;
        public AuthController(IUserAuthRepository user, IAppLogger<AuthController> logger, IForgotPasswordHandler forgotPasswordHandler)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Context cannot be null.");
            _forgotPasswordHandler = forgotPasswordHandler ?? throw new ArgumentNullException(nameof(forgotPasswordHandler), "ForgotPasswordHandler cannot be null.");
        }

        [HttpPost("loginAuth")]
        public async Task<IActionResult> ExternalLogin([FromBody] LoginDTO dto)
        {
            try
            {
                var token = await _user.LoginAsync(dto);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("loginAuth API failed");
                throw;
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenResponseDTO tokens)
        {
            try
            {
                var token = await _user.Refresh(tokens);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("refresh API failed");
                throw;
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var user = await _forgotPasswordHandler.HandleAsync(request);

                if (user == null)
                {
                    return NotFound(new { Error = "User with provided email does not exist." });
                }

                return Ok(new { Message = "New credentials has been sent to the provided Email." });
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("forgot password API failed");
                throw;
            }
        }

    }
}
