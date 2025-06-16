using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Application.DTOs;

namespace TaskManagementWebAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController:ControllerBase
    {
        private readonly IAppLogger<LogsController> _logger;
        public LogsController(IAppLogger<LogsController> logger)
        {
            _logger = logger;
        }

        [HttpPost("PostLog")]
        public IActionResult postLog([FromBody] ClientLogDto logDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (logDto == null)
                return BadRequest("Log data is required");

            var logMessage = $"[Frontend-Logg] {logDto.Timestamp} | {logDto.Url} | {logDto.Message} | Error: {logDto.Error}";
            switch (logDto.Level)
            {
                case "INFO":
                    _logger.LoggInformation(logMessage);
                break;

                case "WARNING":
                    _logger.LoggWarning(logMessage);
                    break;

                case "ERROR":
                    var ex=new Exception(logDto.Error??logDto.Message);
                    _logger.LoggError(ex,logMessage);
                    break;
                default:
                    _logger.LoggInformation(logMessage);
                    break;
            }
            return Ok();
        }
    }
}
