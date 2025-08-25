using LoggingLibrary.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Application.Services.TaskStatusUpdateService;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPITest.Application.Services
{

   public class TaskStatusUpdateApplicationServiceTests
{
    private readonly Mock<ITaskDueStatusUpdateInternalService> _mockTaskStatusService;
    private readonly Mock<ITaskStatusUpdateServiceRepository> _mockRepository;
    private readonly Mock<IAppLogger<TaskStatusUpdateApplicationService>> _mockLogger;
    private readonly TaskStatusUpdateApplicationService _service;

    public TaskStatusUpdateApplicationServiceTests()
    {
        _mockTaskStatusService = new Mock<ITaskDueStatusUpdateInternalService>();
        _mockRepository = new Mock<ITaskStatusUpdateServiceRepository>();
        _mockLogger = new Mock<IAppLogger<TaskStatusUpdateApplicationService>>();

        _service = new TaskStatusUpdateApplicationService(
            _mockTaskStatusService.Object,
            _mockRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void UpdateTaskStatuses_ShouldUpdateStatuses_AndSave()
    {
        // Arrange
        var tasks = new List<Tasks>
        {
             new Tasks{taskId = 1,taskName = "Task 1",dueDate = DateTime.Today.AddDays(1),taskStatus = "NotStarted" },
           new Tasks {taskId = 2,taskName = "Task 2",dueDate = DateTime.Today.AddDays(-1),taskStatus = "NotStarted"}
        };

        _mockRepository.Setup(r => r.GetAllTasks()).Returns(tasks);

        // Act
        _service.UpdateTaskStatuses();

        // Assert
        _mockRepository.Verify(r => r.GetAllTasks(), Times.Once);
        _mockTaskStatusService.Verify(s => s.UpdateTaskStatus(tasks), Times.Once);
        _mockRepository.Verify(r => r.SaveAllTasks(), Times.Once);
    }

    [Fact]
    public void UpdateTaskStatuses_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var ex = new Exception("DB error");
        _mockRepository.Setup(r => r.GetAllTasks()).Throws(ex);

        // Act & Assert
        Assert.Throws<Exception>(() => _service.UpdateTaskStatuses());

        _mockLogger.Verify(l => l.LoggError(ex, "An error occurred while updating task statuses."), Times.Once);
    }
}
}
