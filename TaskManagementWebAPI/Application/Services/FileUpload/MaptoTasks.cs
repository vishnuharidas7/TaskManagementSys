using LoggingLibrary.Interfaces;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.FileUpload
{
    public class MaptoTasks : IMaptoTasks
    {
        public List<Tasks> MapToTasks(List<Dictionary<string, object>> rawData)
        {
            var tasks = new List<Tasks>();
            try
            {
                foreach (var row in rawData)
                {
                    tasks.Add(new Tasks
                    {
                        taskType = row.TryGetValue("TaskType", out var tt) ? tt?.ToString() : null,
                        taskName = row.TryGetValue("TaskName", out var tn) ? tn?.ToString() : null,
                        UserId = row.TryGetValue("AssignedTo", out var uid) && int.TryParse(uid?.ToString(), out var id) ? id : 0,
                        dueDate = row.TryGetValue("DueDate", out var dd) && DateTime.TryParse(dd?.ToString(), out var dt) ? dt : DateTime.MinValue,
                        taskDescription = row.TryGetValue("Description", out var desc) ? desc?.ToString() : null,
                        priority = row.TryGetValue("Priority", out var prio) ? prio?.ToString() : null,
                        createdBy = row.TryGetValue("CreatedBy", out var cb) && int.TryParse(cb?.ToString(), out var cid) ? cid : 0
                    });
                }
            }
            catch (FormatException ex)
            {
                throw;
            }
            catch (ArgumentNullException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            return tasks;
        }
    }
}
