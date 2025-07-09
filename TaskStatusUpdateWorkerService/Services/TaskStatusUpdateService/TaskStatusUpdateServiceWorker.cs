
using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Options;
using Scheduler.Configurations;
using System.Net.Http;
namespace Scheduler.Services.TaskStatusUpdateService
{
    public class TaskStatusUpdateServiceWorker : BackgroundService
    {
    
        private readonly IAppLogger<TaskStatusUpdateServiceWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TaskStatusUpdateServiceWorkerSettings _settings;

        public TaskStatusUpdateServiceWorker(IHttpClientFactory httpClientFactory,IAppLogger<TaskStatusUpdateServiceWorker> logger,
            IOptions<TaskStatusUpdateServiceWorkerSettings> settings)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.PostAsync(_settings.ApiUrlStatusUpdate, null,stoppingToken);

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
                await Task.Delay(TimeSpan.FromMinutes(_settings.TimeStatusUpdate), stoppingToken);
            }
        }

    }
}
