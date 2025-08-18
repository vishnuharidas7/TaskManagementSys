using System.Net.Http;
using System.Text;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Common;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskEmailContentBuilder : IEmailContentBuilder
    {
        private readonly IEnumerable<ITaskStatusContentBuilder> _statusBuilders;
        private readonly ILogger<TaskEmailContentBuilder> _logger;

        public TaskEmailContentBuilder(IEnumerable<ITaskStatusContentBuilder> statusBuilders, ILogger<TaskEmailContentBuilder> logger)
        {
            _statusBuilders = statusBuilders ?? throw new ArgumentNullException(nameof(statusBuilders));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string BuildContent(Users user, IEnumerable<Tasks> tasks)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(MailMessages.GreetingTemplate, user.Name));

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

            sb.AppendLine(MailMessages.Signature);

            return sb.ToString();
        }
    }

}
