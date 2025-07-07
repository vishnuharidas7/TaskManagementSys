using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskFileParser
    {
        // Task<List<Tasks>> ParseAsync(IFormFile file);
        /// <summary>
        /// For parsing data from uploaded file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<List<Dictionary<string, object>>> ParseAsync(IFormFile file);
    }
}
