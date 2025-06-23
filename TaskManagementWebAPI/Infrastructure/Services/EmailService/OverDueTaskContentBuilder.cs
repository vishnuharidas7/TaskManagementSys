using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class OverDueTaskContentBuilder : ITaskStatusContentBuilder
    {
        public string taskState => "OverDue";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder(); 
            sb.AppendLine("⏰ This is a reminder that the following task exceeded due date and require your attention:");
            

            sb.AppendLine("\nTask details : \n");

            foreach (var task in tasks)
                sb.AppendLine($" - Task ID: {task.taskId} {task.taskName} (Due: {task.dueDate:MM/dd/yyyy})");

            sb.AppendLine("\nPlease take action on these as soon as possible.");

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
