using System.Net.Http;

namespace TaskManagementWebAPI.Application.Services.EmailService
{
    public class TaskReminderEmailWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskReminderEmailWorker> _logger;

        public TaskReminderEmailWorker(IServiceProvider serviceProvider, ILogger<TaskReminderEmailWorker> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OverdueTaskEmailWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var dispatcher = scope.ServiceProvider.GetRequiredService<TaskEmailDispatcher>();

                    _logger.LogInformation("Dispatching overdue task emails...");
                    await dispatcher.DispatchEmailsAsync();
                    _logger.LogInformation("Overdue task emails dispatched successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while dispatching overdue task emails.");
                    throw;
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // adjust for testing if needed
            }

            _logger.LogInformation("OverdueTaskEmailWorker is stopping.");
        }
    }
}

     
