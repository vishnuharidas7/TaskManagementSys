using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Controllers;

namespace TaskManagementWebAPITest.Controller
{
    public class LogControllerTest
    {
        private readonly Mock<IAppLogger<LogsController>> _loggerMock;
        private readonly LogsController _logsController;

        public LogControllerTest() {
            _loggerMock= new Mock<IAppLogger<LogsController>>();
            _logsController = new LogsController(
                _loggerMock.Object
                );

        }
        [Fact]
        public void PostLog_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _logsController.ModelState.AddModelError("Message", "Required");
            var logDto = new ClientLogDto();

            // Act
            var result = _logsController.postLog(logDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void PostLog_ReturnsBadRequest_WhenLogDTONull()
        {
            // Arrange
            var logDto = (ClientLogDto)null;

            // Act
            var result = _logsController.postLog(logDto);

            // Assert
            var badRequest= Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Log data is required", badRequest.Value);
          
        }
        [Fact]
        public void PostLog_ReturnsOk_ForValidInfoLog()
        {
            // Arrange
            var logDto = new ClientLogDto
            {
                Level = "INFO",
                Message = "This is an info log.",
                Url = "/home",
                Timestamp = DateTime.UtcNow.ToString(),
                Error = null
            };

            // Act
            var result = _logsController.postLog(logDto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _loggerMock.Verify(logger => logger.LoggInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void PostLog_ReturnsOk_ForValidWarningLog()
        {
            // Arrange
            var logDto = new ClientLogDto
            {
                Level = "WARNING",
                Message = "This is a warning.",
                Url = "/dashboard",
                Timestamp = DateTime.UtcNow.ToString(),
                Error = null
            };

            // Act
            var result = _logsController.postLog(logDto);

            // Assert
            Assert.IsType<OkResult>(result);
            _loggerMock.Verify(logger => logger.LoggWarning(It.IsAny<string>()), Times.Once);
        }
        [Fact]
        public void PostLog_ReturnsOk_ForValidErrorLog()
        {
            // Arrange
            var logDto = new ClientLogDto
            {
                Level = "ERROR",
                Message = "Something went wrong.",
                Url = "/error",
                Timestamp = DateTime.UtcNow.ToString(),
                Error = "NullReferenceException"
            };

            // Act
            var result = _logsController.postLog(logDto);

            // Assert
            Assert.IsType<OkResult>(result);
            _loggerMock.Verify(logger => logger.LoggError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
        [Fact]
        public void PostLog_LogsInfo_WhenLevelIsUnknown()
        {
            // Arrange
            var logDto = new ClientLogDto
            {
                Level = "UNKNOWN",
                Message = "Unknown level log.",
                Url = "/unknown",
                Timestamp = DateTime.UtcNow.ToString(),
                Error = null
            };

            // Act
            var result = _logsController.postLog(logDto);

            // Assert
            Assert.IsType<OkResult>(result);
            _loggerMock.Verify(logger => logger.LoggInformation(It.IsAny<string>()), Times.Once);
        }

    }
}
