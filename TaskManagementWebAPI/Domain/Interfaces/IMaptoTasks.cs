using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IMaptoTasks
    {
        List<Tasks> MapToTasks(List<Dictionary<string, object>> rawData);
        
    }
}
