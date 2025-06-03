using Microsoft.AspNetCore.Http;
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

        public AuthController(IUserAuthRepository user)
        {
            _user = user;
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
                throw;
            }
        }

    }
}
