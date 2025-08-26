using LoggingLibrary.Interfaces;
using System.Globalization;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.FileUpload
{
    public class MaptoTasks : IMaptoTasks
    {
        public List<Tasks> MapToTasks(List<Dictionary<string, object>> rawData, Dictionary<string, int> userNameToId, int createdUserId)
        {
            var tasks = new List<Tasks>();
            try
            {
                foreach (var row in rawData)
                {
                    string? assignedTo = row.TryGetValue(ExcelHeaders.AssignedTo, out var at) ? at?.ToString() : null;
                    int userId = 0;
                    if (!string.IsNullOrWhiteSpace(assignedTo) && userNameToId.TryGetValue(assignedTo.ToLower(), out var resolvedId))
                    {
                        userId = resolvedId;
                    }
                    tasks.Add(new Tasks
                    {
                        taskType = row.TryGetValue(ExcelHeaders.TaskType, out var tt) ? tt?.ToString() ?? "" : "",
                        taskName = row.TryGetValue(ExcelHeaders.TaskName, out var tn) ? tn?.ToString() ?? "" : "",
                        UserId = userId,
                        //dueDate = row.TryGetValue(ExcelHeaders.DueDate, out var dd) && DateTime.TryParse(dd?.ToString(), out var dt) ? dt : DateTime.MinValue,
                        dueDate = row.TryGetValue(ExcelHeaders.DueDate, out var dd) && DateTime.TryParse(dd?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)? dt: DateTime.MinValue,
                        taskDescription = row.TryGetValue(ExcelHeaders.Description, out var desc) ? desc?.ToString() ?? "" : "",
                        priority = row.TryGetValue(ExcelHeaders.Priority, out var prio) ? prio?.ToString() ?? "" : "",
                        createdBy = createdUserId
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
