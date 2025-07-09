
using LoggingLibrary.Interfaces;
using System.Net.Http;
namespace Scheduler.Services.TaskStatusUpdateService
{
    public class TaskStatusUpdateServiceWorker : BackgroundService
    {
    
        private readonly IAppLogger<TaskStatusUpdateServiceWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;


        public TaskStatusUpdateServiceWorker(IHttpClientFactory httpClientFactory,IServiceScopeFactory scopeFactory, IAppLogger<TaskStatusUpdateServiceWorker> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.PostAsync("https://localhost:7192/api/Tasks/update-statuses-scheduler", null,stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LoggInformation("Task status update successful at: {time}", DateTimeOffset.Now);
                    }
                    else
                    {
                        _logger.LoggWarning("ask status update failed with status code: {code}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LoggError(ex, "Exception during task status update");
                    //throw;
                }


                // Delay for 24 hours (or adjust for testing)
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

    }
}
