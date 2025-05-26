using TaskManagementWebAPI.DTOs;

namespace TaskManagementWebAPI.Repositories
{
    public interface IAssignUserRepository
    {
        Task<List<AssignUserDTO>> ViewUsers();

        Task AddTask(AddTaskDTO dto);

        Task ProcessExcelFileAsync(IFormFile file);
    }
}
