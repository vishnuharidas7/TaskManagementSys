using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class NewTaskContentBuilder : ITaskStatusContentBuilder
    {
        public string taskState => "New";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("🆕 New Tasks Assigned:");

            foreach (var task in tasks)
                sb.AppendLine($" - Task ID: {task.referenceId} + {task.taskType} +  {task.taskName}" +
                    $" (Due: {task.dueDate:MM/dd/yyyy}) " +
                    $"(Priotity: {task.priority})");

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
