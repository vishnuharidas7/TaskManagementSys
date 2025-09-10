using Microsoft.AspNetCore.Http; 

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskFileParser
    {
        /// <summary>
        /// For parsing data from uploaded file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<List<Dictionary<string, object>>> ParseAsync(IFormFile file);
    }
}
