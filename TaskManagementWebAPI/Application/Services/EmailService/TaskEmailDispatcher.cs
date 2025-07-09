using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskEmailDispatcher
    {
        private readonly IUserEmailRepository _userRepo;
        private readonly ITaskEmailRepository _taskRepo;
        private readonly IEmailService _emailService;
        private readonly IEmailContentBuilder _contentBuilder;
        private readonly ILogger<TaskEmailDispatcher> _logger;

        public TaskEmailDispatcher(
            IUserEmailRepository userRepo,
            ITaskEmailRepository taskRepo,
            IEmailService emailService,
            IEmailContentBuilder contentBuilder,
            ILogger<TaskEmailDispatcher> logger)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _taskRepo = taskRepo ?? throw new ArgumentNullException(nameof(taskRepo));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _contentBuilder = contentBuilder ?? throw new ArgumentNullException(nameof(contentBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DispatchEmailsAsync()
        {
            var users = _userRepo.GetAllUsers();

            foreach (var user in users)
            {
                try
                {
                    var tasks = _taskRepo.GetTasksByUserId(user.UserId)
                        .Where(t => (t.taskState == "Due" || t.taskState == "OverDue") && t.taskStatus != "Completed")
                        .ToList();

                    if (tasks.Any())
                    {
                        string content = _contentBuilder.BuildContent(user, tasks);
                        await _emailService.SendEmailAsync(user.Email, "Task Completion Reminder — Action Required", content);

                        _logger.LogInformation("📧 Email sent to {Email} with {TaskCount} tasks.", user.Email, tasks.Count);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "❌ Invalid operation for user {UserId} ({Email})", user.UserId, user.Email);
                    throw;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "❌ Network error while sending email to {UserId} ({Email})", user.UserId, user.Email);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unexpected error while sending email to user {UserId} ({Email})", user.UserId, user.Email);
                    throw;
                }
            }
        }
    }
}
