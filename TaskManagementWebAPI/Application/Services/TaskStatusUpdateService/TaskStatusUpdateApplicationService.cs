using LoggingLibrary.Interfaces;
using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Application.Services.TaskStatusUpdateService
{
    public class TaskStatusUpdateApplicationService : ITaskDueStatusUpdateService
    {
        private readonly TaskDueStatusUpdateService _taskStatusService;
        private readonly ITaskStatusUpdateServiceRepository _taskRepository;
        private readonly IAppLogger<TaskDueStatusUpdateService> _logger;

        public TaskStatusUpdateApplicationService(TaskDueStatusUpdateService taskStatusService, ITaskStatusUpdateServiceRepository taskRepository, IAppLogger<TaskDueStatusUpdateService> logger)
        {
            _taskStatusService = taskStatusService ?? throw new ArgumentNullException(nameof(taskStatusService), "taskStatusService cannot be null.");
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository), "taskRepository cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Context cannot be null.");
        }

        public void UpdateTaskStatuses()
        {
            try
            {
                var tasks = _taskRepository.GetAllTasks();
                _taskStatusService.UpdateTaskStatus(tasks);
                _taskRepository.SaveAllTasks();
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "An error occurred while updating task statuses.");
                throw;
            }

        }
    }
}
