using TaskManagement_Project.DTOs;
using TaskManagementWebAPI.DTOs;

namespace TaskManagementWebAPI.Repositories
{
    public interface IAssignUserRepository
    {
        Task<List<AssignUserDTO>> ViewUsers();

        Task AddTask(AddTaskDTO dto);

        Task ProcessExcelFileAsync(IFormFile file);

        Task<List<ViewTasksDTO>> viewAllTasks();

        Task DeleteTask(int id);

        Task UpdateTask(int id, AddTaskDTO obj);
    }
}
