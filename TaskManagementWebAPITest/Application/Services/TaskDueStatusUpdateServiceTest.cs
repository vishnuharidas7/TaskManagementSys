using LoggingLibrary.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagementWebAPI.Application.Services.TaskStatusUpdateService;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPITest.Application.Services
{
    public class TaskDueStatusUpdateServiceTests
    {
        private readonly Mock<IAppLogger<TaskDueStatusUpdateService>> _mockLogger;
        private readonly TaskDueStatusUpdateService _service;

        public TaskDueStatusUpdateServiceTests()
        {
            _mockLogger = new Mock<IAppLogger<TaskDueStatusUpdateService>>();
            _service = new TaskDueStatusUpdateService(_mockLogger.Object);
        }

        public class FaultyTask : Tasks
        {
            public override void UpdateStateToDue()
            {
                throw new InvalidOperationException("Simulated exception");
            }
        }



        [Fact]
        public void Updates_TaskState_To_Due_When_Due_In_2_Days()
        {
            var task = new Tasks
            {
                dueDate = DateTime.Today.AddDays(2),
                taskStatus = "New"
            };

            _service.UpdateTaskStatus(new List<Tasks> { task });

            Assert.Equal("Due", task.taskState);
        }

        [Fact]
        public void Updates_TaskState_To_OverDue_When_DueDate_Is_Past()
        {
            var task = new Tasks
            {
                dueDate = DateTime.Today.AddDays(-1),
                taskStatus = "InProgress"
            };

            _service.UpdateTaskStatus(new List<Tasks> { task });

            Assert.Equal("OverDue", task.taskState);
        }

        [Fact]
        public void Does_Not_Update_TaskState_If_Status_Is_Completed()
        {
            var task = new Tasks
            {
                dueDate = DateTime.Today.AddDays(-3),
                taskStatus = "Completed",
                taskState = "New"
            };

            _service.UpdateTaskStatus(new List<Tasks> { task });

            Assert.Equal("New", task.taskState);
        }

        
        [Fact]
        public void Logs_Error_And_Throws_When_Exception_Occurs()
        {
            var faultyTask = new FaultyTask
            {
                dueDate = DateTime.Today,
                taskStatus = "New",
                taskId = 1
            };

            var tasks = new List<Tasks> { faultyTask };

            var ex = Assert.Throws<InvalidOperationException>(() => _service.UpdateTaskStatus(tasks));

            Assert.Equal("Simulated exception", ex.Message);

            _mockLogger.Verify(
                x => x.LoggError(It.IsAny<Exception>(),
                                 It.Is<string>(msg => msg.Contains("Failed to update status for task ID"))),
                Times.Once);
        }


    }
}
