using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Repositories;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private ITaskManagementRepository _user;
        private readonly ILogger<TasksController> _logger;
        public TasksController(ITaskManagementRepository user, ILogger<TasksController> logger)
        {
            _user = user;
            _logger =logger;
        }
        [HttpGet("AssignUser")]
        public async Task<ActionResult> assignUserList()
        {
            try
            {
                var allUser = await _user.ViewUsers();
                return Ok(allUser);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("AssignUser API faild");
                throw;
            }
        }

        [HttpGet("ViewAllTasks")]
        public async Task<ActionResult> viewAllTask()
        {
            try
            {
                var allTasks = await _user.viewAllTasks();
                return Ok(allTasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("ViewAllTasks API faild");
                throw;
            }
        }

        [HttpPost("AddTask")]
        public async Task<IActionResult> AddTask(AddTaskDTO dto)
        {
            try
            {
                await _user.AddTask(dto);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("AddTask API faild");
                throw;
            }
        }

        [HttpPut("UpdateTask/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] AddTaskDTO obj)
        {
            try
            {
                await _user.UpdateTask(id, obj);
                return Ok(obj);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("UpdateTask API faild");
                throw;
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");


                await _user.ProcessExcelFileAsync(file);
                return Ok("File processed and tasks saved.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("upload API faild");
                throw;
            }
        }

        [HttpDelete("deleteTask/{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            try
            {
                await _user.DeleteTask(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("deleteTask API faild");
                throw;
            }

        }

        [HttpGet("task/{userId:int}")]
        public async Task<IEnumerable<AddTaskDTO>> GetTasksByUserId(int userId)
        {
            try
            {
                var userTask = await _user.GetTasksByUserId(userId);
                return userTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("task API faild");
                throw;
            }
        }
    }
}
