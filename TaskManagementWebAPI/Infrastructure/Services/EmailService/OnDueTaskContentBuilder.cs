using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class OnDueTaskContentBuilder : ITaskStatusContentBuilder
    {
        public string taskState => "Due";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder();
             
            sb.AppendLine("⏰ Gentle reminder, Task due dates are approaching.");
             
            sb.AppendLine("\nTask details : \n");

            foreach (var task in tasks)
                sb.AppendLine($" - Task ID: {task.referenceId} {task.taskType} {task.taskName} (Due: {task.dueDate:MM/dd/yyyy})");
           
            sb.AppendLine("\nPlease take action on these as soon as possible.");

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
