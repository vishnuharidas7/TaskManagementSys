using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface ITaskStatusContentBuilder
    {
        string taskState { get; }
        /// <summary>
        /// To build Email content
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        string BuildSection(IEnumerable<Tasks> tasks);
    }
}
