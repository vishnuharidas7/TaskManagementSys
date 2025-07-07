using Microsoft.AspNetCore.Http;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
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
