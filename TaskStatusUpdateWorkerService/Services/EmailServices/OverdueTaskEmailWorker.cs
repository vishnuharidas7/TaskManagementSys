//Shceduler

using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Services.EmailServices
{
    public class OverdueTaskEmailWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppLogger<OverdueTaskEmailWorker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public OverdueTaskEmailWorker(IHttpClientFactory httpClientFactory,IServiceProvider serviceProvider, IAppLogger<OverdueTaskEmailWorker> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LoggInformation("OverdueTaskEmailWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
         
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.PostAsync(" https://localhost:7192/api/Tasks/update-overduetaskmail-scheduler", null, stoppingToken);

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

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // adjust for testing if needed
            }

            _logger.LoggInformation("OverdueTaskEmailWorker is stopping.");
        }
    }
}
