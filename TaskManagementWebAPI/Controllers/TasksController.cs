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
        private readonly ITaskApplicartionService _taskControllerService;
        private readonly ITaskEmailDispatcher _taskEmailDispatcher;
        private readonly TTaskApplicationServices _taskApplicationService;

        public TasksController(ITaskManagementRepository task, IAppLogger<TasksController> logger, TTaskApplicationServices taskAppService,ITaskEmailDispatcher taskEmailDispatcher, ITaskApplicartionService taskControllerService)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task), "Task cannot be null.");
            _logger =logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            _taskApplicationService = taskAppService;
            _taskEmailDispatcher=taskEmailDispatcher;
            _taskControllerService = taskControllerService ?? throw new ArgumentNullException(nameof(_taskControllerService),"TaskControlService cannot be null.");
        }

        /// <summary>
        /// For fetching user name in the dropdown for assigning task
        /// </summary>
        /// <returns></returns>
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
        /// For viewing all the tasks details
        /// </summary>
        /// <returns></returns>
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
        /// For adding a new task into the system
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
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
        /// To update task details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,User")]
        [HttpPut("UpdateTask/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] AddTaskDTO obj)
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
        /// To upload bulk task details into the system
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
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
        /// To delete a specific task detail from the system
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// To fetch user details corresponding to the user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
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
        /// To fetch notifications data regarding tasks for user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
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
        /// To fetch notification data regarding tasks for admin
        /// </summary>
        /// <returns></returns>
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

        //scheduler
        //[Authorize(Roles = "Admin,User")]
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

        //scheduler
        //[Authorize(Roles = "Admin,User")]
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
