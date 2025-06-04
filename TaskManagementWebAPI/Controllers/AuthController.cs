using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Repositories;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthRepository _user;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IUserAuthRepository user, ILogger<AuthController> logger)
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
                _logger.LogWarning("loginAuth API faild");
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
                _logger.LogWarning("refresh API faild");
                throw;
            }
        }

    }
}
