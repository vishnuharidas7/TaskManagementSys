using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
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
        public AuthController(IUserAuthRepository user, IAppLogger<AuthController> logger)
        {
            _user = user;
            _logger = logger;
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

    }
}
