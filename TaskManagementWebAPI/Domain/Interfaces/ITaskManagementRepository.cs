using Microsoft.AspNetCore.Mvc; 
using TaskManagementWebAPI.Application.DTOs;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskManagementRepository
    {
        Task<List<AssignUserDTO>> ViewUsers();

        Task AddTask(AddTaskDTO dto);

        //Task ProcessExcelFileAsync(IFormFile file);

        Task ProcessFileAsync(IFormFile file);

        Task<List<ViewTasksDTO>> viewAllTasks();

        Task DeleteTask(int id);

        Task UpdateTask(int id, AddTaskDTO obj);

        // Task<ActionResult<AddTaskDTO>> GetTaskById(int id);
        //Task<ActionResult<IEnumerable<AddTaskDTO>>> GetTasksByUserId(int userId);
        Task<IEnumerable<ViewTasksDTO>> GetTasksByUserId(int userId);
        Task<IEnumerable<NotificationDTO>> GetTasksNotificationByUserId(int userId);

        Task<List<NotificationDTO>> GetTasksNotificationbByAdmin();
    }
}
