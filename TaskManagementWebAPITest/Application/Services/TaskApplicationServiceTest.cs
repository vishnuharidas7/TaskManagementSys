using LoggingLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Application.Services;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;
using TaskManagementWebAPI.Infrastructure.Repositories;

namespace TaskManagementWebAPITest.Application.Services
{
    public class TaskApplicationServiceTest
    {
        private readonly TaskApplicationService _taskApplicationService;
        private readonly Mock<ITaskManagementRepository> _taskManagementRepositoryMock;
        private readonly Mock<IAppLogger<UserAuthRepository>> _loggerMock;
        private readonly IOptions<TaskSettings> _taskSettings;
        private readonly Mock<IEmailContentBuilder> _contentBuilderMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ITaskFileParserFactory> _parserFactoryMock;
        private readonly Mock<IMaptoTasks> _taskMapperMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public TaskApplicationServiceTest()
        {
            _taskManagementRepositoryMock = new Mock<ITaskManagementRepository>();
            _loggerMock = new Mock<IAppLogger<UserAuthRepository>>();
            _taskSettings = Options.Create(new TaskSettings { IDTaskPrefix = "TMS", InitialReferenceId = 1001 });
            _emailServiceMock = new Mock<IEmailService>();
            _contentBuilderMock = new Mock<IEmailContentBuilder>();
            _parserFactoryMock = new Mock<ITaskFileParserFactory>();
            _taskMapperMock = new Mock<IMaptoTasks>();
            _configurationMock = new Mock<IConfiguration>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _taskApplicationService = new TaskApplicationService(
                _taskManagementRepositoryMock.Object,
                _loggerMock.Object,
                _taskSettings,
                _contentBuilderMock.Object,
                _emailServiceMock.Object,
                _parserFactoryMock.Object,
                _taskMapperMock.Object,
                _configurationMock.Object,
                _userRepositoryMock.Object
            );
        }

        private AddTaskDTO GetSampleDTO() => new AddTaskDTO
        {
            taskId=1,
            taskName = "Test-Task",
            UserId = 1,
            dueDate = DateTime.UtcNow.AddDays(7),
            taskDescription = "Test Description",
            priority = "High",
            taskStatus="New",
            userName="amal",
            createdBy = 1,
            taskType = "Feature"
        };

        private Tasks GetSampleTask() => new Tasks
        {
            taskId = 1,
            referenceId = "TMS-1001"
        };

        private DbUpdateException GetDuplicateDbUpdateException()
        {
            var inner = new Exception("Duplicate entry for key 'referenceId'");
            return new DbUpdateException("Duplicate", inner);
        }

        [Fact]
        public async Task AddTaskAsync_SuccessfullyAddsTaskAndSendsEmail()
        { 
            // Arrange
            var dto = GetSampleDTO();
            var task = GetSampleTask();

            _taskManagementRepositoryMock.Setup(r => r.LastTaskWithPrefix(It.IsAny<string>())).ReturnsAsync((Tasks)null);
            _taskManagementRepositoryMock.Setup(r => r.AddTask(It.IsAny<Tasks>())).ReturnsAsync(task.taskId);
            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(dto.UserId)).ReturnsAsync(new Users {Email  = "test@gmail.com" });
            _taskManagementRepositoryMock.Setup(r => r.GetTasksByTaskIdAsync(task.taskId)).ReturnsAsync(new List<Tasks> { task });
            _contentBuilderMock.Setup(c => c.BuildContent(It.IsAny<Users>(), It.IsAny<IEnumerable<Tasks>>())).Returns("Email Content");

            //act
            await _taskApplicationService.AddTaskAsync(dto);

            //assert    
            _emailServiceMock.Verify(e => e.SendEmailAsync("test@gmail.com", "New Task Added", "Email Content"), Times.Once);
        }

        [Fact]
        public async Task AddTaskAsync_UserNotFound_LogsWarning()
        {
            // Arrange
            var dto = GetSampleDTO();
            var task = GetSampleTask();

            _taskManagementRepositoryMock.Setup(r => r.LastTaskWithPrefix(It.IsAny<string>())).ReturnsAsync((Tasks)null);
            _taskManagementRepositoryMock.Setup(r => r.AddTask(It.IsAny<Tasks>())).ReturnsAsync(task.taskId);
            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(dto.UserId)).ReturnsAsync((Users)null);

            //act
            await _taskApplicationService.AddTaskAsync(dto);

            //assert 
            _loggerMock.Verify(l => l.LoggWarning("User not found for ID {UserId}", dto.UserId), Times.Once);
        }

        [Fact]
        public async Task AddTaskAsync_RetriesOnDuplicateReferenceId()
        {
            // Arrange
            var dto = GetSampleDTO();
            var task = GetSampleTask();
            int callCount = 0;

            _taskManagementRepositoryMock.Setup(r => r.LastTaskWithPrefix(It.IsAny<string>())).ReturnsAsync((Tasks)null);
            _taskManagementRepositoryMock.Setup(r => r.AddTask(It.IsAny<Tasks>()))
                .Returns(() =>
                {
                    if (callCount++ < 2)
                        throw GetDuplicateDbUpdateException();
                    return Task.FromResult(task.taskId);
                });

            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(dto.UserId)).ReturnsAsync(new Users { Email = "test@gmail.com"});
            _taskManagementRepositoryMock.Setup(r => r.GetTasksByTaskIdAsync(task.taskId)).ReturnsAsync(new List<Tasks> { task });
            _contentBuilderMock.Setup(c => c.BuildContent(It.IsAny<Users>(), It.IsAny<IEnumerable<Tasks>>())).Returns("Email");

            //act
            await _taskApplicationService.AddTaskAsync(dto);

            //assert 
            _loggerMock.Verify(log =>
                log.LoggWarning("Duplicate reference ID generated — retrying... Attempt {Attempt}", It.IsAny<object[]>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task AddTaskAsync_ThrowsAfterMaxRetries()
        {
            
            var dto = GetSampleDTO();
            _taskManagementRepositoryMock.Setup(r => r.LastTaskWithPrefix(It.IsAny<string>())).ReturnsAsync((Tasks)null);
            _taskManagementRepositoryMock.Setup(r => r.AddTask(It.IsAny<Tasks>())).Throws(GetDuplicateDbUpdateException());
         
            var ex = await Assert.ThrowsAsync<Exception>(() => _taskApplicationService.AddTaskAsync(dto));
          
            Assert.Contains("Failed to add task after multiple attempts", ex.Message);
            _loggerMock.Verify(log =>
                log.LoggWarning("Duplicate reference ID generated — retrying... Attempt {Attempt}", It.IsAny<object[]>()),
                Times.Exactly(5));
        }

        [Fact]
        public async Task AddTaskAsync_HandlesInvalidOperationException()
        {
            
            var dto = GetSampleDTO();
            var inner = new Exception("Inner");
            var ex = new InvalidOperationException("Invalid", inner);
            _taskManagementRepositoryMock.Setup(r => r.LastTaskWithPrefix(It.IsAny<string>())).Throws(ex);
           
            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => _taskApplicationService.AddTaskAsync(dto));
         
            Assert.Equal(ex, thrown);
        }

        [Fact]
        public async Task AddTaskAsync_HandlesDbUpdateException()
        {
           
            var dto = GetSampleDTO();
            var inner = new Exception("Inner DB");
            var ex = new DbUpdateException("DB Error", inner);
            _taskManagementRepositoryMock.Setup(r => r.LastTaskWithPrefix(It.IsAny<string>())).ReturnsAsync((Tasks)null);
            _taskManagementRepositoryMock.Setup(r => r.AddTask(It.IsAny<Tasks>())).Throws(ex);
            
            var thrown = await Assert.ThrowsAsync<DbUpdateException>(() => _taskApplicationService.AddTaskAsync(dto));
            
            Assert.Equal(ex, thrown);
        }

        [Fact]
        public async Task AddTaskAsync_HandlesGeneralException()
        {
            
            var dto = GetSampleDTO();
            var ex = new Exception("Unexpected");
            _taskManagementRepositoryMock.Setup(r => r.LastTaskWithPrefix(It.IsAny<string>())).ReturnsAsync((Tasks)null);
            _taskManagementRepositoryMock.Setup(r => r.AddTask(It.IsAny<Tasks>())).Throws(ex);
            
            var thrown = await Assert.ThrowsAsync<Exception>(() => _taskApplicationService.AddTaskAsync(dto));
           
            Assert.Equal(ex, thrown);
        }
    }
}
