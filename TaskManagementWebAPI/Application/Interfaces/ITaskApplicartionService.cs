using TaskManagementWebAPI.Application.DTOs;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface ITaskApplicartionService
    {
        /// <summary>
        /// For Adding a new task
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task AddTaskAsync(AddTaskDTO dto);

        /// <summary>
        /// For updating task status by the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task UpdateTask(int id, AddTaskDTO obj);

        /// <summary>
        /// For Exracting and saving the data from the uploaded file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task ProcessFileAsync(IFormFile file);

    }
}
