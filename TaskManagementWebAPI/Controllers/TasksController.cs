using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private ITaskManagementRepository _task;
        private readonly IAppLogger<TasksController> _logger;
        private readonly ITaskApplicationService _taskControllerService;
        private readonly ITaskEmailDispatcher _taskEmailDispatcher;
        private readonly ITaskDueStatusUpdateService _taskApplicationService;

        public TasksController(ITaskManagementRepository task, IAppLogger<TasksController> logger, ITaskDueStatusUpdateService taskAppService,ITaskEmailDispatcher taskEmailDispatcher, ITaskApplicationService taskControllerService)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task), "Task cannot be null.");
            _logger =logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            _taskApplicationService = taskAppService;
            _taskEmailDispatcher=taskEmailDispatcher;
            _taskControllerService = taskControllerService ?? throw new ArgumentNullException(nameof(_taskControllerService),"TaskControlService cannot be null.");
        }

        /// <summary>
        /// Retrieves a list of users to populate the assign-to dropdown for tasks.
        /// </summary>
        /// <param></param>
        /// <returns>List of users (ID and Name)</returns>
        /// <response code="200">Successfully fetched user list</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("AssignUser")]
        public async Task<ActionResult> assignUserList()
        {
            try
            {
                var allUser = await _task.ViewUsers();
                return Ok(allUser);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("AssignUser API failed");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of all task details including assigned user information.
        /// </summary>
        /// <param></param>
        /// <returns>List of tasks</returns>
        /// <response code="200">Successfully fetched task list</response>
        /// <response code = "403" > Forbidden - user lacks permission</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("ViewAllTasks")]
        public async Task<ActionResult> viewAllTask()
        {
            try
            {
                var allTasks = await _task.viewAllTasks();
                return Ok(allTasks);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("ViewAllTasks API failed");
                throw;
            }
        }

        /// <summary>
        /// Adds a new task to the system.
        /// </summary>
        /// <param name="dto">Task details to add</param>
        /// <returns>HTTP 200 OK if successful</returns>
        /// <response code="200">Task added successfully</response>
        /// <response code="400">Bad request or validation failure</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpPost("AddTask")]
        public async Task<IActionResult> AddTask(AddTaskDTO dto)
        {
            try
            {
                //await _task.AddTask(dto);
                await _taskControllerService.AddTaskAsync(dto);
                _logger.LoggWarning("AddTask success");
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("AddTask API failed");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing User with the given ID.
        /// </summary>
        /// <param name="id">ID of the User to update</param>
        /// <param name="obj">Updated User details</param>
        /// <returns>HTTP 200 OK if successful</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Bad request or invalid data</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpPut("UpdateTask/{id}")]
        public async Task<ActionResult> UpdateUserTask(int id, [FromBody] AddTaskDTO obj)
        {
            try
            {
                //await _task.UpdateTask(id, obj);
                await _taskControllerService.UpdateTask(id, obj);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("UpdateTask API failed");
                throw;
            }
        }

        /// <summary>
        /// Uploads and processes a task file. Only accessible by Admins.
        /// </summary>
        /// <param name="file">The file to upload and process (CSV or Excel)</param>
        /// <returns>HTTP 200 OK if the file is successfully processed</returns>
        /// <response code="200">File processed and tasks saved</response>
        /// <response code="400">Bad request - missing file or invalid content</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");


                await _taskControllerService.ProcessFileAsync(file);
                return Ok("File processed and tasks saved.");
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("upload API failed");
                throw;
            }
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to delete</param>
        /// <returns>HTTP 200 OK if deleted successfully</returns>
        /// <response code="200">Task deleted successfully</response>
        /// <response code="400">Bad request (invalid ID)</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="404">Task not found</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpDelete("deleteTask/{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            try
            {
                await _task.DeleteTask(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("deleteTask API failed");
                throw;
            }

        }

        /// <summary>
        /// Retrieves all tasks assigned to a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A list of task details assigned to the user</returns>
        /// <response code="200">Successfully retrieved tasks</response>
        /// <response code="400">Bad request - invalid user ID</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("task/{userId:int}")]
        public async Task<IEnumerable<ViewTasksDTO>> GetTasksByUserId(int userId)
        {
            try
            {
                var userTask = await _task.GetTasksByUserId(userId);
                return userTask;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("task API failed");
                throw;
            }
        }

        /// <summary>
        /// Retrieves upcoming task notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>List of task notifications due in the next 2 days</returns>
        /// <response code="200">Successfully retrieved notifications</response>
        /// <response code="400">Bad request - invalid user ID</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("taskNotification/{userId:int}")]
        public async Task<IEnumerable<NotificationDTO>> GetTasksNotificationbByUserId(int userId)
        {
            try
            {
                var userTask = await _task.GetTasksNotificationByUserId(userId);
                return userTask;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("task API failed");
                throw;
            }
        }

        /// <summary>
        /// Retrieves task notifications (due in 2 days) for all users – visible only to Admin.
        /// </summary>
        /// <returns>List of notifications with task and user details</returns>
        /// <response code="200">Successfully retrieved notifications</response>
        /// <response code="403">Forbidden - user lacks permission</response>
        /// <response code="500">Internal server error</response>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("taskNotificationAdmin")]
        public async Task<IEnumerable<NotificationDTO>> GetTasksNotificationbByAdmin()
        {
            try
            {
                var userTask = await _task.GetTasksNotificationbByAdmin();
                return userTask;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("task API failed");
                throw;
            }
        }

        /// <summary>
        /// Updates statuses of all tasks in the system.
        /// </summary>
        /// <returns>HTTP 200 OK if successful</returns>
        /// <response code="200">Task statuses updated successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("update-Taskstatuses")]
        public IActionResult UpdateTaskStatuses()
        {
            try
            {
                _taskApplicationService.UpdateTaskStatuses();
                _logger.LoggInformation("Task statuses updated via API");
                return Ok("Task statuses updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Error updating task statuses");
                return StatusCode(500, "Error updating tasks.");
            }
        }

        /// <summary>
        /// Sends email notifications for overdue tasks to users.
        /// </summary>
        /// <returns>HTTP 200 OK if emails dispatched successfully</returns>
        /// <response code="200">Emails dispatched successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("send-overduetaskmail")]
        public IActionResult OverdueTaskEmail()
        {
            try
            {
                _taskEmailDispatcher.DispatchEmailsAsync();
                _logger.LoggInformation("Task over due mail generated via API");
                return Ok("Task statuses updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "ErrorTask over due mail generating");
                return StatusCode(500, "Error ver due mail generating.");
            }
        }

    }
}
