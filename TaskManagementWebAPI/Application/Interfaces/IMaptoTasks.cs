using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface IMaptoTasks
    {
        /// <summary>
        /// For mapping task details after extraction
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        List<Tasks> MapToTasks(List<Dictionary<string, object>> rawData);
        
    }
}
