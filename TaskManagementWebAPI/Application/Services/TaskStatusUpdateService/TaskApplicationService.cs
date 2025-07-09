using LoggingLibrary.Interfaces;
using System.Net.Http;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Application.Services.TaskStatusUpdateService
{
    public class TaskApplicationService
    {
        private readonly TaskStatusService _taskStatusService;
        private readonly ITaskStatusRepository _taskRepository;
        private readonly IAppLogger<TaskApplicationService> _logger;

        public TaskApplicationService(TaskStatusService taskStatusService, ITaskStatusRepository taskRepository, IAppLogger<TaskApplicationService> logger)
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
