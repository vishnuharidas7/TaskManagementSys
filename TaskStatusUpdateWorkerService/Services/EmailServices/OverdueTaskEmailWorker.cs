//Shceduler

using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Options;
using Scheduler.Configurations;

namespace Scheduler.Services.EmailServices
{
    public class OverdueTaskEmailWorker : BackgroundService
    {
        private readonly IAppLogger<OverdueTaskEmailWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OverdueTaskEmailWorkerSettings _settings;
        public OverdueTaskEmailWorker(IHttpClientFactory httpClientFactory,IAppLogger<OverdueTaskEmailWorker> logger
             ,IOptions<OverdueTaskEmailWorkerSettings> settings)
        {
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
                  // throw;         
                }

                await Task.Delay(TimeSpan.FromMinutes(_settings.TimeTaskEmail), stoppingToken); // adjust for testing if needed
            }

            _logger.LoggInformation("OverdueTaskEmailWorker is stopping.");
        }
    }
}
