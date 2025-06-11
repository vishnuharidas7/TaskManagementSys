using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; 
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private ITaskManagementRepository _task;
        private readonly IAppLogger<TasksController> _logger;
        public TasksController(ITaskManagementRepository task, IAppLogger<TasksController> logger)
        {
            _task = task;
            _logger =logger;
        }
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
                _logger.LoggWarning("AssignUser API faild");
                throw;
            }
        }

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
                _logger.LoggWarning("ViewAllTasks API faild");
                throw;
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost("AddTask")]
        public async Task<IActionResult> AddTask(AddTaskDTO dto)
        {
            try
            {
                await _task.AddTask(dto);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("AddTask API faild");
                throw;
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPut("UpdateTask/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] AddTaskDTO obj)
        {
            try
            {
                await _task.UpdateTask(id, obj);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("UpdateTask API faild");
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");


                await _task.ProcessFileAsync(file);
                return Ok("File processed and tasks saved.");
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("upload API faild");
                throw;
            }
        }


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
                _logger.LoggWarning("deleteTask API faild");
                throw;
            }

        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("task/{userId:int}")]
        public async Task<IEnumerable<AddTaskDTO>> GetTasksByUserId(int userId)
        {
            try
            {
                var userTask = await _task.GetTasksByUserId(userId);
                return userTask;
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("task API faild");
                throw;
            }
        }
    }
}
