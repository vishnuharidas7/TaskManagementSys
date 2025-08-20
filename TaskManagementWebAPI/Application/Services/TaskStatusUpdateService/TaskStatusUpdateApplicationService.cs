using LoggingLibrary.Interfaces;
using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Application.Services.TaskStatusUpdateService
{
    public class TaskStatusUpdateApplicationService : ITaskDueStatusUpdateService
    {
        private readonly ITaskDueStatusUpdateInternalService _taskStatusService;
        private readonly ITaskStatusUpdateServiceRepository _taskRepository;
        private readonly IAppLogger<TaskStatusUpdateApplicationService> _logger;

        public TaskStatusUpdateApplicationService(ITaskDueStatusUpdateInternalService taskStatusService, ITaskStatusUpdateServiceRepository taskRepository, IAppLogger<TaskStatusUpdateApplicationService> logger)
        {
            _taskStatusService = taskStatusService ?? throw new ArgumentNullException(nameof(taskStatusService));
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
