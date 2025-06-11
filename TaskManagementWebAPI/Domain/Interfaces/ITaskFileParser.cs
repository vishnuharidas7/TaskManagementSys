using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskFileParser
    {
        // Task<List<Tasks>> ParseAsync(IFormFile file);
        Task<List<Dictionary<string, object>>> ParseAsync(IFormFile file);
    }
}
