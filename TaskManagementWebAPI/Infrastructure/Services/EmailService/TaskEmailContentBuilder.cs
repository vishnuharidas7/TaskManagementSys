using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class TaskEmailContentBuilder : IEmailContentBuilder
    {
        private readonly IEnumerable<ITaskStatusContentBuilder> _statusBuilders;

        public TaskEmailContentBuilder(IEnumerable<ITaskStatusContentBuilder> statusBuilders)
        {
            _statusBuilders = statusBuilders;
        }

        public string BuildContent(Users user, IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Hey {user.Name},\n");

            var grouped = tasks.GroupBy(t => t.taskStatus);

            foreach (var group in grouped)
            {
                var builder = _statusBuilders.FirstOrDefault(b => b.taskStatus == group.Key);
                if (builder != null)
                {
                    sb.AppendLine(builder.BuildSection(group));
                }
            }
             
            sb.AppendLine("Regards,\nTask Management System");

            return sb.ToString();
        }
    }
    //: IEmailContentBuilder
    //{
    //    public string BuildContent(Users user, IEnumerable<Tasks> tasks)
    //    {
    //        var sb = new StringBuilder();
    //        sb.AppendLine($"Hello {user.Name},");
    //        sb.AppendLine("Here are your current tasks:");

    //        foreach (var task in tasks)
    //        {
    //            sb.AppendLine($"- {task.taskName} [{task.taskStatus}] - Due: {task.dueDate:MM/dd/yyyy}");
    //        }

    //        sb.AppendLine("\nRegards,\nTask Management System");
    //        return sb.ToString();
    //    }
    //}
}
