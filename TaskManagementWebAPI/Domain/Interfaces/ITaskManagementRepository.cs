using Microsoft.AspNetCore.Mvc; 
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Models;

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
        Task<int> AddTask(Tasks task);

        /// <summary>
        /// For saving extracted data from uploaded file using dapper
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        Task SaveTasksWithDapperAsync(List<Tasks> tasks);

        /// <summary>
        /// For saving extracted data from uploaded file using EF
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        Task SaveTasksWithEFAsync(List<Tasks> tasks, string prefix);

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
        Task UpdateTask(Tasks task);

        
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

        /// <summary>
        /// For fetching task with taskId for sending mail
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<List<Tasks>> GetTasksByTaskIdAsync(int taskId);
        /// <summary>
        /// For fetching Last Task
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        Task<Tasks> LastTaskWithPrefix(string prefix);
        /// <summary>
        /// Fetch Task with Id
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<Tasks> TaskWithIdFindAsync(int taskId);
        /// <summary>
        /// Fetch all task by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<Tasks> GetAllTasksByUserId(int userId);
    }
}
