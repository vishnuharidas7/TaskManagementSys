using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.FileUpload
{
    public class MaptoTasks : IMaptoTasks
    {
        public List<Tasks> MapToTasks(List<Dictionary<string, object>> rawData)
        {
            var tasks = new List<Tasks>();

            foreach (var row in rawData)
            {
                tasks.Add(new Tasks
                {
                    taskName = row.TryGetValue("TaskName", out var tn) ? tn?.ToString() : null,
                    UserId = row.TryGetValue("UserId", out var uid) && int.TryParse(uid?.ToString(), out var id) ? id : 0,
                    dueDate = row.TryGetValue("DueDate", out var dd) && DateTime.TryParse(dd?.ToString(), out var dt) ? dt : DateTime.MinValue,
                    taskDescription = row.TryGetValue("Description", out var desc) ? desc?.ToString() : null,
                    priority = row.TryGetValue("Priority", out var prio) ? prio?.ToString() : null,
                    createdBy = row.TryGetValue("CreatedBy", out var cb) && int.TryParse(cb?.ToString(), out var cid) ? cid : 0
                });
            }

            return tasks;
        }
    }
}
