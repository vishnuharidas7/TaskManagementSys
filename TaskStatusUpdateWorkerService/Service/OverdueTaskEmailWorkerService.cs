using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Configurations;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler.Services.EmailServices
{
    public class OverdueTaskEmailWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppLogger<OverdueTaskEmailWorkerService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OverdueTaskEmailWorkerSettings _settings;

        public OverdueTaskEmailWorkerService(
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider,
            IAppLogger<OverdueTaskEmailWorkerService> logger,
            IOptions<OverdueTaskEmailWorkerSettings> settings)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LoggInformation("OverdueTaskEmailWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.PostAsync(_settings.ApiUrlTaskEmail, null, stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LoggInformation("OverdueTaskEmail work successful at: {time}", DateTimeOffset.Now);
                    }
                    else
                    {
                        _logger.LoggWarning("OverdueTaskEmail work failed with status code: {code}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LoggError(ex, "Exception during OverdueTaskEmail working");
                }

                await Task.Delay(TimeSpan.FromMinutes(_settings.TimeTaskEmail), stoppingToken);
            }

            _logger.LoggInformation("OverdueTaskEmailWorker is stopping.");
        }
    }
}
