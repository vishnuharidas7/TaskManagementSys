using TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService;

namespace TaskManagementWebAPI.Infrastructure.Services.TaskOndueUpdate
{
    public class TaskStatusUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TaskStatusUpdateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var taskAppService = scope.ServiceProvider.GetRequiredService<TaskApplicationService>();
                    taskAppService.UpdateTaskStatuses(); // or await if async
                }

                // Delay for 24 hours (or adjust for testing)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
        //private readonly TaskApplicationService _taskApplicationService;

        //public TaskStatusUpdateService(TaskApplicationService taskApplicationService)
        //{
        //    _taskApplicationService = taskApplicationService;
        //}

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        _taskApplicationService.UpdateTaskStatuses();
        //        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        //    }
        //}
    }
}
