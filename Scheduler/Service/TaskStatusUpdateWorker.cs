using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Options;
using Scheduler.Configurations;
using Scheduler.Services.EmailServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Service
{
    public class TaskStatusUpdateWorker: BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppLogger<TaskStatusUpdateWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TaskStatusUpdateWorkerSettings _settings;

        public TaskStatusUpdateWorker(
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider,
            IAppLogger<TaskStatusUpdateWorker> logger,
            IOptions<TaskStatusUpdateWorkerSettings> settings)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LoggInformation("OverdueTaskStatusUpdateWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.PostAsync(_settings.ApiUrlStatusUpdate, null, stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LoggInformation("OverdueTaskStatusUpdateWorker work successful at: {time}", DateTimeOffset.Now);
                    }
                    else
                    {
                        _logger.LoggWarning("OverdueTaskStatusUpdateWorker Response failed", response.IsSuccessStatusCode);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LoggError(ex, "HTTP request failed OverdueTaskStatusUpdateWorker. Ensure the API is running and accessible at {url}", _settings.ApiUrlStatusUpdate);
                }
                catch (Exception ex)
                {
                    _logger.LoggError(ex, "Exception during OverdueTaskStatusUpdateWorker working");
                }

                await Task.Delay(TimeSpan.FromMinutes(_settings.TimeStatusUpdate), stoppingToken);
            }

            _logger.LoggInformation("OverdueTaskStatusUpdateWorker is stopping.");
        }
    }
}
