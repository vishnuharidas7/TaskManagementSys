using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagement_Project.DTOs;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Repositories;

namespace TaskManagementWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private IAssignUserRepository _user;
        public TasksController(IAssignUserRepository user)
        {
            _user = user;
        }
        [HttpGet("AssignUser")]
        public async Task<ActionResult> assignUserList()
        {
            var allUser = await _user.ViewUsers();
            return Ok(allUser);
        }

        [HttpGet("ViewAllTasks")]
        public async Task<ActionResult> viewAllTask()
        {
            var allTasks = await _user.viewAllTasks();
            return Ok(allTasks);
        }

        [HttpPost("AddTask")]
        public async Task<IActionResult> AddTask(AddTaskDTO dto)
        {  
            await _user.AddTask(dto);
            return Ok(dto);
        }

        [HttpPut("UpdateTask/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] AddTaskDTO obj)
        {
            await _user.UpdateTask(id, obj);
            return Ok(obj);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            //if (file == null || file.Length == 0)
            //    return BadRequest("No file uploaded.");

            //var result = await _user.ProcessExcelFileAsync(file);

            //return Ok(result);
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                await _user.ProcessExcelFileAsync(file);
                return Ok("File processed and tasks saved.");
            }
            catch (Exception ex)
            {
                return BadRequest("Error processing file: " + ex.Message);
            }
        }

        [HttpDelete("deleteTask/{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            await _user.DeleteTask(id);
            return Ok();

        }
    }
}
