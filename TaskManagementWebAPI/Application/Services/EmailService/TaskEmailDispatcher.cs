using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskEmailDispatcher: ITaskEmailDispatcher
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskNotificationService _taskNotificationService;
        private readonly ILogger<TaskEmailDispatcher> _logger;
        private readonly ITaskManagementRepository _taskManagementRepository;

        public TaskEmailDispatcher(
            IUserRepository userRepository,
            ITaskNotificationService taskNotificationService,
            ITaskManagementRepository taskManagementRepository,
            ILogger<TaskEmailDispatcher> logger)
        {
            _userRepository = userRepository;
            _taskNotificationService = taskNotificationService ?? throw new ArgumentNullException(nameof(taskNotificationService));
            _taskManagementRepository = taskManagementRepository??throw new ArgumentNullException(nameof(taskManagementRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DispatchEmailsAsync()
        {
            var users = _userRepository.GetAllUsers();

            foreach (var user in users)
            {
                try
                {
                    var tasks = _taskManagementRepository.GetAllTasksByUserId(user.UserId)
                        .Where(t => (t.taskState == "Due" || t.taskState == "OverDue") && t.taskStatus != "Completed")
                        .ToList();

                    if (tasks.Any())
                    {
                        //string content = _contentBuilder.BuildContent(user, tasks);
                        //await _emailService.SendEmailAsync(user.Email, "Task Completion Reminder — Action Required", content);
                        var content= _taskNotificationService.SendNotificationAsync(user, tasks);

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
