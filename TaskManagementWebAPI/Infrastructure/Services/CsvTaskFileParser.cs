using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services
{
    public class CsvTaskFileParser : ITaskFileParser
    {
        // public async Task<List<Tasks>> ParseAsync(IFormFile file)
        public async Task<List<Dictionary<string, object>>> ParseAsync(IFormFile file)
        {
            throw new NotImplementedException("Not implemented at the moment...");
        }
    }
}
