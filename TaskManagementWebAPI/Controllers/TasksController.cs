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
        private ITaskManagementRepository _user;
        public TasksController(ITaskManagementRepository user)
        {
            _user = user;
        }
        [HttpGet("AssignUser")]
        public async Task<ActionResult> assignUserList()
        {
            try
            {
                var allUser = await _user.ViewUsers();
                return Ok(allUser);
            }
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
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
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
