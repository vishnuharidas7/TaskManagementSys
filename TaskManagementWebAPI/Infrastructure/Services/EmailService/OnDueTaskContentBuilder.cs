using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class OnDueTaskContentBuilder : ITaskStatusContentBuilder
    {
        public string taskStatus => "OnDue";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder();
            var onDueTasks = tasks
               .Where(task =>
                              (task.dueDate.Date - DateTime.Today).Days >= 0)
               .ToList();

            if (onDueTasks.Any()) 
            {
                sb.AppendLine("⏰ Gentle reminder, Task due dates are approaching.");
            }

            else
                sb.AppendLine("⏰ This is a reminder that the following tasks are on due and require your attention:");
            // sb.AppendLine();

            sb.AppendLine("\nTask details : \n");

            foreach (var task in tasks)
                sb.AppendLine($" - {task.taskName} (Due: {task.dueDate:MM/dd/yyyy})");
           
            sb.AppendLine("\nPlease take action on these as soon as possible.");

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
