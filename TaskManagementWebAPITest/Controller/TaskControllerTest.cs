using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Controllers;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPITest.Controller
{
    public class TaskControllerTest
    {
        private readonly TasksController _Taskcontroller;
        private readonly Mock<ITaskManagementRepository> _taskRepoMock;
        private readonly Mock<IAppLogger<TasksController>> _loggerMock;
        private readonly Mock<ITaskDueStatusUpdateService> _statusUpdateServiceMock;
        private readonly Mock<ITaskEmailDispatcher> _emailDispatcherMock;
        private readonly Mock<ITaskApplicationService> _taskAppServiceMock;

        public TaskControllerTest() // Inject mock
        {
            _taskRepoMock = new Mock<ITaskManagementRepository>();
            _loggerMock = new Mock<IAppLogger<TasksController>>();
            _statusUpdateServiceMock = new Mock<ITaskDueStatusUpdateService>();
            _emailDispatcherMock = new Mock<ITaskEmailDispatcher>();
            _taskAppServiceMock = new Mock<ITaskApplicationService>();
            _Taskcontroller = new TasksController(
               _taskRepoMock.Object,
               _loggerMock.Object,
               _statusUpdateServiceMock.Object,
               _emailDispatcherMock.Object,
               _taskAppServiceMock.Object
           );//Inject that mock in controller

        }



        [Fact]
        public async Task AssignUserList_ReturnOk_WithListOfAllUsers()
        {
            var mockAssignUser = new List<AssignUserDTO>
            {
            new AssignUserDTO
            {
                Id = 1,
                Name = "Amal",  
            }
            };

            _taskRepoMock.Setup(repo => repo.ViewUsers()).ReturnsAsync(mockAssignUser);
            //Act
            var result = await _Taskcontroller.assignUserList();
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);//means HTTP 200 OK with data.--Assert check if something is true.Assertion fails, the test fails./OkObjectResult-return Ok(myTasks);->This returns an OkObjectResult
            var returnValue = Assert.IsType<List<AssignUserDTO>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal(1, returnValue[0].Id);
            Assert.Equal("Amal", returnValue[0].Name);
        }

        [Fact]
        public async Task ViewAllTsk_ReturnOk_WithListOfTasks()
        {
            var mockTasks = new List<ViewTasksDTO>
            {
            new ViewTasksDTO
            {
                taskId = 1,
                taskName = "Bug-111",
                userId = 3,
                userName = "Tijo",
                taskDescription = "Bug-111",
                dueDate = DateTime.Today.AddDays(2),
                taskStatus = "New",
                priority = "High",
                taskType = "Bug",
                referenceId = "TMS-1000",
                taskState = "New"
            }
            };

            _taskRepoMock.Setup(repo => repo.viewAllTasks()).ReturnsAsync(mockTasks);
            //Act
            var result = await _Taskcontroller.viewAllTask();
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);//means HTTP 200 OK with data.--Assert check if something is true.Assertion fails, the test fails./OkObjectResult-return Ok(myTasks);->This returns an OkObjectResult
            var returnValue=Assert.IsType<List<ViewTasksDTO>>(okResult.Value);
            Assert.Single(returnValue); 
            Assert.Equal(1, returnValue[0].taskId);
            Assert.Equal("Bug-111", returnValue[0].taskName);
            Assert.Equal(3, returnValue[0].userId);
            Assert.Equal("Tijo", returnValue[0].userName);
            Assert.Equal("Bug-111", returnValue[0].taskDescription);
            Assert.Equal(DateTime.Today.AddDays(2), returnValue[0].dueDate);
            Assert.Equal("New", returnValue[0].taskStatus);
            Assert.Equal("High", returnValue[0].priority);
            Assert.Equal("Bug", returnValue[0].taskType);
            Assert.Equal("TMS-1000", returnValue[0].referenceId);
            Assert.Equal("New", returnValue[0].taskState);
        }

        [Fact]
        public async Task AddTask_ValidDto_ReturnsOkWithDto()
        {
            // Arrange
            var dto = new AddTaskDTO
            {
                taskName = "Test-Task",
                taskDescription = "Test Description",
                UserId = 1,
                dueDate = System.DateTime.Today.AddDays(3),
                priority = "High",
                createdBy = 1,
                taskType = "Bug",
                taskId = 12,
                taskStatus = "New",
                userName = "amal",
            };
                 
            _taskAppServiceMock.Setup(service => service.AddTaskAsync(dto))
                .Returns(Task.CompletedTask); // No exception thrown

            // Act
            var result = await _Taskcontroller.AddTask(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, okResult.Value);

            // Optional,Verify AddTaskAsync was called once
            _taskAppServiceMock.Verify(s => s.AddTaskAsync(dto), Times.Once);
        }

        [Fact]
        public async Task UpdateUserTask_ValidInput_CallsServiceAndReturnsOk()
        {
            // Arrange
            var taskId = 1;
            var dto = new AddTaskDTO
            {
                taskName = "Test-Task123",
                taskDescription = "Test Description 123",
                UserId = 1,
                dueDate = System.DateTime.Today.AddDays(3),
                priority = "High",
                createdBy = 1,
                taskType = "Bug",
                taskId = 12,
                taskStatus = "New",
                userName = "amal",
            };

            _taskAppServiceMock
                .Setup(s => s.UpdateTask(taskId, dto))
                .Returns(Task.CompletedTask); // Simulate success

            // Act
            var result = await _Taskcontroller.UpdateUserTask(taskId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, okResult.Value);

            _taskAppServiceMock.Verify(s => s.UpdateTask(taskId, dto), Times.Once);
        }

        //Case 1: Valid file upload
        [Fact]
        public async Task FileUpload_ValidFile_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var fileMock = new Mock<IFormFile>();
            var content = "Task content here";
            var fileName = "tasks.csv";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.ContentType).Returns("text/csv");

            _taskAppServiceMock
                .Setup(s => s.ProcessFileAsync(userId, fileMock.Object))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _Taskcontroller.FileUpload(userId, fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File processed and tasks saved.", okResult.Value);

            _taskAppServiceMock.Verify(s => s.ProcessFileAsync(userId, fileMock.Object), Times.Once);
        }
        //Case 2: File is null or empty
        [Fact]
        public async Task FileUpload_NullOrEmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var emptyFileMock = new Mock<IFormFile>();
            emptyFileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _Taskcontroller.FileUpload(userId, emptyFileMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);

            _taskAppServiceMock.Verify(s => s.ProcessFileAsync(It.IsAny<int>(), It.IsAny<IFormFile>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTask_ValidId_ReturnsOk()
        {
            // Arrange
            var taskId = 1;

            _taskRepoMock
                .Setup(s => s.DeleteTask(taskId))
                .Returns(Task.CompletedTask); // Simulate successful deletion

            // Act
            var result = await _Taskcontroller.DeleteTask(taskId);

            // Assert
            Assert.IsType<OkResult>(result); // Just checking 200 OK

            _taskRepoMock.Verify(s => s.DeleteTask(taskId), Times.Once);
        }


        [Fact]
        public async Task GetTasksByUserId_ValidUserId_ReturnsTasks()
        {
            // Arrange
            int userId = 1;

            var mockTasks = new List<ViewTasksDTO>
            {
              new ViewTasksDTO
              {
               taskId = 101,
               taskName = "Test Task",
               userId = userId,
               userName = "John",
               taskDescription = "Test description",
               dueDate = DateTime.Today.AddDays(1),
               taskStatus = "New",
               priority = "High",
               taskType = "Bug",
               referenceId = "TMS-001",
               taskState = "New"
               }
             };

            _taskRepoMock
                .Setup(s => s.GetTasksByUserId(userId))
                .ReturnsAsync(mockTasks);

            // Act
            var result = await _Taskcontroller.GetTasksByUserId(userId);

            // Assert
            Assert.NotNull(result);
            var taskList = Assert.IsAssignableFrom<IEnumerable<ViewTasksDTO>>(result);
            var task = taskList.First();

            Assert.Single(taskList);
            Assert.Equal(101, task.taskId);
            Assert.Equal("Test Task", task.taskName);
            Assert.Equal(userId, task.userId);
            Assert.Equal("John", task.userName);

            _taskRepoMock.Verify(s => s.GetTasksByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task GetTasksNotificationbByUserId_ReturnsNotificationList()
        {
            // Arrange
            int userId = 1;
            var mockNotifications = new List<NotificationDTO>
            {
               new NotificationDTO
               {
                 TaskId  = 1,
                 TaskName  = "TaskA12",
                 DueDate = DateTime.Today.AddDays(1),
                 TaskStatus="New",
                 UserName="amal",
                 referenceId="TMS-1000"
               },
               new NotificationDTO
               {
                 TaskId  = 2,
                 TaskName  = "TaskAA11",
                 DueDate = DateTime.Today.AddDays(-1),
                 TaskStatus="New",
                 UserName="amal",
                 referenceId="TMS-1001"
               }
            };

            _taskRepoMock
               .Setup(service => service.GetTasksNotificationByUserId(userId))
                .ReturnsAsync(mockNotifications);

            // Act
            var result = await _Taskcontroller.GetTasksNotificationbByUserId(userId);

            // Assert
            Assert.NotNull(result);
            var notifications = Assert.IsAssignableFrom<IEnumerable<NotificationDTO>>(result);
            Assert.Equal(2, notifications.Count());
            Assert.Contains(notifications, n => n.TaskId == 1);
            Assert.Contains(notifications, n => n.TaskId == 2);

            // Verify the method was called exactly once
            _taskRepoMock.Verify(service => service.GetTasksNotificationByUserId(userId), Times.Once);
        }


        [Fact]
        public async Task GetTasksNotificationbByAdmin_ReturnsNotificationList()
        {
            // Arrange
            var mockNotifications = new List<NotificationDTO>
            {
              new NotificationDTO
              {
                 TaskId  = 11,
                 TaskName  = "TaskA12",
                 DueDate = DateTime.Today.AddDays(1),
                 TaskStatus="New",
                 UserName="amal",
                 referenceId="TMS-1000"
              },
              new NotificationDTO
              {
                 TaskId  = 12,
                 TaskName  = "TaskAA11",
                 DueDate = DateTime.Today.AddDays(2),
                 TaskStatus="New",
                 UserName="amal",
                 referenceId="TMS-1001"
              }
            };

            _taskRepoMock
                .Setup(service => service.GetTasksNotificationbByAdmin())
                .ReturnsAsync(mockNotifications);

            // Act
            var result = await _Taskcontroller.GetTasksNotificationbByAdmin();

            // Assert
            Assert.NotNull(result);
            var notifications = Assert.IsAssignableFrom<IEnumerable<NotificationDTO>>(result);
            Assert.Equal(2, notifications.Count());
            Assert.Contains(notifications, n => n.TaskId == 11);
            Assert.Contains(notifications, n => n.TaskId == 12);

            _taskRepoMock.Verify(service => service.GetTasksNotificationbByAdmin(), Times.Once);
        }

        [Fact]
        public void UpdateTaskStatuses_CallsServiceAndReturnsOk()
        {
            // Arrange
            _statusUpdateServiceMock.Setup(service => service.UpdateTaskStatuses());

            // Act
            var result = _Taskcontroller.UpdateTaskStatuses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task statuses updated successfully.", okResult.Value);

            _statusUpdateServiceMock.Verify(service => service.UpdateTaskStatuses(), Times.Once);
            _loggerMock.Verify(logger => logger.LoggInformation("Task statuses updated via API"), Times.Once);
        }

        [Fact]
        public void OverdueTaskEmail_CallsDispatcherAndReturnsOk()
        {
            // Arrange
            _emailDispatcherMock
                .Setup(dispatcher => dispatcher.DispatchEmailsAsync());

            // Act
            var result = _Taskcontroller.OverdueTaskEmail();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task statuses updated successfully.", okResult.Value);

            _emailDispatcherMock.Verify(dispatcher => dispatcher.DispatchEmailsAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LoggInformation("Task over due mail generated via API"), Times.Once);
        }








    }
}
