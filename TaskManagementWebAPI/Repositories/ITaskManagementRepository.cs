using TaskManagementWebAPI.DTOs;

namespace TaskManagementWebAPI.Repositories
{
    public interface ITaskManagementRepository
    {
        Task<List<AssignUserDTO>> ViewUsers();

        Task AddTask(AddTaskDTO dto);

        Task ProcessExcelFileAsync(IFormFile file);

        Task<List<ViewTasksDTO>> viewAllTasks();

        Task DeleteTask(int id);

        Task UpdateTask(int id, AddTaskDTO obj);

        // Task<ActionResult<AddTaskDTO>> GetTaskById(int id);
        //Task<ActionResult<IEnumerable<AddTaskDTO>>> GetTasksByUserId(int userId);
        Task<IEnumerable<AddTaskDTO>> GetTasksByUserId(int userId);
    }
}
