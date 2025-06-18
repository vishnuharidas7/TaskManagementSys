using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService
{
    public class TaskApplicationService
    {
        private readonly TaskStatusService _taskStatusService;
        private readonly ITaskStatusRepository _taskRepository;

        public TaskApplicationService(TaskStatusService taskStatusService, ITaskStatusRepository taskRepository)
        {
            _taskStatusService = taskStatusService;
            _taskRepository = taskRepository;
        }

        public void UpdateTaskStatuses()
        {
            var tasks = _taskRepository.GetAllTasks();
            _taskStatusService.UpdateTaskStatus(tasks);
            _taskRepository.SaveAllTasks();
        }
    }
}
