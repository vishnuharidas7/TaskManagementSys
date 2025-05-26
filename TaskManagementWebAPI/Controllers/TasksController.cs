using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("AddTask")]
        public async Task<IActionResult> AddTask(AddTaskDTO dto)
        {  
            await _user.AddTask(dto);
            return Ok(dto);
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
    }
}
