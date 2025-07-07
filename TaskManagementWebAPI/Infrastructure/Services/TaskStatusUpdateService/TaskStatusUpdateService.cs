using LoggingLibrary.Interfaces;
using TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService;

namespace TaskManagementWebAPI.Infrastructure.Services.TaskOndueUpdate
{
    public class TaskStatusUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAppLogger<TaskStatusUpdateService> _logger;


        public TaskStatusUpdateService(IServiceScopeFactory scopeFactory,IAppLogger<TaskStatusUpdateService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory), "scopeFactory cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var taskAppService = scope.ServiceProvider.GetRequiredService<TaskApplicationService>();
                        taskAppService.UpdateTaskStatuses(); // or await if async
                    }

                }
                catch (Exception ex)
                {
                    _logger.LoggError(ex, $"❌ Error updating task statuses: {ex.Message}");
                    Console.WriteLine($"❌ Error updating task statuses: {ex.Message}");
                    throw;
                    
                }

                // Delay for 24 hours (or adjust for testing)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
     
    }
}
