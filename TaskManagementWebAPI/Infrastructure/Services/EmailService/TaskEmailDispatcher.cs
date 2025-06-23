using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class TaskEmailDispatcher
    {
        private readonly IUserEmailRepository _userRepo;
        private readonly ITaskEmailRepository _taskRepo;
        private readonly IEmailService _emailService;
        private readonly IEmailContentBuilder _contentBuilder;

        public TaskEmailDispatcher(
            IUserEmailRepository userRepo,
            ITaskEmailRepository taskRepo,
            IEmailService emailService,
            IEmailContentBuilder contentBuilder)
        {
            _userRepo = userRepo;
            _taskRepo = taskRepo;
            _emailService = emailService;
            _contentBuilder = contentBuilder;
        }

        public async Task DispatchEmailsAsync()
        {
            foreach (var user in _userRepo.GetAllUsers())
            {
                var tasks = _taskRepo.GetTasksByUserId(user.UserId)
                    .Where(t => t.taskState == "Due" || t.taskState== "OverDue")
                    .ToList();

                if (tasks.Any())
                {
                    var content = _contentBuilder.BuildContent(user, tasks);
                    await _emailService.SendEmailAsync(user.Email, "Task Completion Reminder — Action Required", content);
                }
            }
        }
    }
}
