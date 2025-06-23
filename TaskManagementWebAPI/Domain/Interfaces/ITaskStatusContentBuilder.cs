using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskStatusContentBuilder
    {
        string taskState { get; }
        string BuildSection(IEnumerable<Tasks> tasks);
    }
}
