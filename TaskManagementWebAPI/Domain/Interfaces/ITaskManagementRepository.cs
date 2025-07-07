using Microsoft.AspNetCore.Mvc; 
using TaskManagementWebAPI.Application.DTOs;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskManagementRepository
    {
        /// <summary>
        /// To view user name in the dropdown for assigning task 
        /// </summary>
        /// <returns></returns>
        Task<List<AssignUserDTO>> ViewUsers();

        /// <summary>
        /// Added for creating a new task by user or admin.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task AddTask(AddTaskDTO dto);

        /// <summary>
        /// For extracting and saving the uploaded file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task ProcessFileAsync(IFormFile file);

        /// <summary>
        /// For viewing all the tasks details
        /// </summary>
        /// <returns></returns>
        Task<List<ViewTasksDTO>> viewAllTasks();

        /// <summary>
        /// For deleting a task specific task
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteTask(int id);

        /// <summary>
        /// For updating task status by assigned user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task UpdateTask(int id, AddTaskDTO obj);

        
        /// <summary>
        /// To view tasks assigned to a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<ViewTasksDTO>> GetTasksByUserId(int userId);

        /// <summary>
        /// For fetching User notification details
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<NotificationDTO>> GetTasksNotificationByUserId(int userId);

        /// <summary>
        /// For fetching Admin notification details
        /// </summary>
        /// <returns></returns>
        Task<List<NotificationDTO>> GetTasksNotificationbByAdmin();
    }
}
