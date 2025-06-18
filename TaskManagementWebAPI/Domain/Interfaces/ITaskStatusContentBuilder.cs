using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskStatusContentBuilder
    {
        string taskStatus { get; }
        string BuildSection(IEnumerable<Tasks> tasks);
    }
}
