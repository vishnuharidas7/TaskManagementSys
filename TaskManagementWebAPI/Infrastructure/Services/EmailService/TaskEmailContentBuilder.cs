using System.Net.Http;
using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class TaskEmailContentBuilder : IEmailContentBuilder
    {
        private readonly IEnumerable<ITaskStatusContentBuilder> _statusBuilders;
        private readonly ILogger<TaskEmailContentBuilder> _logger;

        public TaskEmailContentBuilder(IEnumerable<ITaskStatusContentBuilder> statusBuilders, ILogger<TaskEmailContentBuilder> logger)
        {
            _statusBuilders = statusBuilders ?? throw new ArgumentNullException(nameof(statusBuilders), "statusBuilders cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
        }

        public string BuildContent(Users user, IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Hey {user.Name},\n");

            var grouped = tasks.GroupBy(t => t.taskState);

            foreach (var group in grouped)
            {
                try
                {
                    var builder = _statusBuilders.FirstOrDefault(b => b.taskState == group.Key);

                    if (builder != null)
                    {
                        sb.AppendLine(builder.BuildSection(group));
                    }
                    else
                    {
                        _logger.LogWarning("No content builder found for taskState: {TaskState}", group.Key);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "❌ Invalid operation while building email section for taskState: {TaskState}", group.Key);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unexpected error building email section for taskState: {TaskState}", group.Key);
                    throw;
                }
            }

            sb.AppendLine("Regards,\nTask Management System");

            return sb.ToString();
        }
    }

}
