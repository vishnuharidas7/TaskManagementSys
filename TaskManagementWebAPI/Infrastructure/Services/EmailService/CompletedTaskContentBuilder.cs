using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class CompletedTaskContentBuilder : ITaskStatusContentBuilder
    {
        public string taskStatus => "Completed";

        public string BuildSection(IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("✅ Completed Tasks:");

            foreach (var task in tasks)
                sb.AppendLine($" - {task.taskName} (Due: {task.dueDate:MM/dd/yyyy})");

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
