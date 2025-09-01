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

            foreach (var row in rawData)
            {
                var task = MapRowToTask(row, userNameToId, createdUserId);
                tasks.Add(task);
            }

            return tasks;
        }

        private Tasks MapRowToTask(Dictionary<string, object> row, Dictionary<string, int> userNameToId, int createdUserId)
        {
            string assignedTo = GetStringValue(row, ExcelHeaders.AssignedTo);
            int userId = ResolveUserId(assignedTo, userNameToId);

            return new Tasks
            {
                taskType = GetStringValue(row, ExcelHeaders.TaskType),
                taskName = GetStringValue(row, ExcelHeaders.TaskName),
                UserId = userId,
                dueDate = GetDateValue(row, ExcelHeaders.DueDate),
                taskDescription = GetStringValue(row, ExcelHeaders.Description),
                priority = GetStringValue(row, ExcelHeaders.Priority),
                createdBy = createdUserId
            };
        }

        private string GetStringValue(Dictionary<string, object> row, string key)
        {
            return row.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";
        }

        private DateTime GetDateValue(Dictionary<string, object> row, string key)
        {
            if (row.TryGetValue(key, out var value) &&
                DateTime.TryParse(value?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return DateTime.MinValue;
        }

        private int ResolveUserId(string assignedTo, Dictionary<string, int> userNameToId)
        {
            if (!string.IsNullOrWhiteSpace(assignedTo) &&
                userNameToId.TryGetValue(assignedTo.ToLower(), out var userId))
            {
                return userId;
            }
            return 0;
        }

    }
}
